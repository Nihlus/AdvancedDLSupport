//
//  StringMarshallingWrapper.cs
//
//  Author:
//       Jarl Gullberg <jarl.gullberg@gmail.com>
//
//  Copyright (c) Jarl Gullberg
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU Lesser General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU Lesser General Public License for more details.
//
//  You should have received a copy of the GNU Lesser General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using AdvancedDLSupport.Extensions;
using AdvancedDLSupport.Pipeline;
using AdvancedDLSupport.Reflection;
using JetBrains.Annotations;
using StrictEmit;

using static AdvancedDLSupport.ImplementationGenerators.GeneratorComplexity;
using static System.Runtime.InteropServices.UnmanagedType;

#pragma warning disable SA1513

namespace AdvancedDLSupport.ImplementationGenerators
{
    /// <summary>
    /// Generates wrapper instructions for marshalling string parameters, with an optional attribute-controlled
    /// cleanup step to free the marshalled memory afterwards.
    /// </summary>
    internal sealed class StringMarshallingWrapper : CallWrapperBase
    {
        /// <inheritdoc/>
        public override GeneratorComplexity Complexity => MemberDependent | TransformsParameters;

        /// <summary>
        /// Holds local variables defined for a given work unit. The nested dictionary contains the 0-based input
        /// parameter index matched with the local variable containing an unmanaged pointer.
        /// </summary>
        [NotNull]
        private Dictionary<PipelineWorkUnit<IntrospectiveMethodInfo>, Dictionary<int, LocalBuilder>> _workUnitLocals
            = new Dictionary<PipelineWorkUnit<IntrospectiveMethodInfo>, Dictionary<int, LocalBuilder>>();

        [NotNull]
        private static Dictionary<UnmanagedType, MethodInfo> _stringToPtrMethods;

        [NotNull]
        private static Dictionary<UnmanagedType, MethodInfo> _ptrToStringMethods;

        [NotNull]
        private static MethodInfo _freeBStrMethod;

        [NotNull]
        private static MethodInfo _freeHGlobalMethod;

        [NotNull]
        private static MethodInfo _freeCoTaskMemMethod;

        [CanBeNull]
        private static UnmanagedType? _utf8UnmanagedType;

        static StringMarshallingWrapper()
        {
            _stringToPtrMethods = new Dictionary<UnmanagedType, MethodInfo>();
            _ptrToStringMethods = new Dictionary<UnmanagedType, MethodInfo>();

            // Managed-to-unmanaged methods
            _stringToPtrMethods.Add
            (
                BStr,
                typeof(Marshal).GetMethod
                (
                    nameof(Marshal.StringToBSTR),
                    new[] { typeof(string) }
                )
            );

            _stringToPtrMethods.Add
            (
                LPWStr,
                typeof(Marshal).GetMethod
                (
                    nameof(Marshal.StringToHGlobalUni),
                    new[] { typeof(string) }
                )
            );

            _stringToPtrMethods.Add
            (
                LPStr,
                typeof(Marshal).GetMethod
                (
                    nameof(Marshal.StringToHGlobalAnsi),
                    new[] { typeof(string) }
                )
            );

            _stringToPtrMethods.Add
            (
                LPTStr,
                typeof(Marshal).GetMethod
                (
                    nameof(Marshal.StringToHGlobalAuto),
                    new[] { typeof(string) }
                )
            );

            // Unmanaged-to-managed methods
            _ptrToStringMethods.Add
            (
                BStr,
                typeof(Marshal).GetMethod
                (
                    nameof(Marshal.PtrToStringBSTR),
                    new[] { typeof(IntPtr) }
                )
            );

            _ptrToStringMethods.Add
            (
                LPWStr,
                typeof(Marshal).GetMethod
                (
                    nameof(Marshal.PtrToStringUni),
                    new[] { typeof(IntPtr) }
                )
            );

            _ptrToStringMethods.Add
            (
                LPStr,
                typeof(Marshal).GetMethod
                (
                    nameof(Marshal.PtrToStringAnsi),
                    new[] { typeof(IntPtr) }
                )
            );

            _ptrToStringMethods.Add
            (
                LPTStr,
                typeof(Marshal).GetMethod
                (
                    nameof(Marshal.PtrToStringAuto),
                    new[] { typeof(IntPtr) }
                )
            );

            // Add UTF8 string support, if available.
            var utf8UnmanagedTypeField = typeof(UnmanagedType).GetField("LPUTF8Str");
            if (!(utf8UnmanagedTypeField is null))
            {
                _utf8UnmanagedType = (UnmanagedType)utf8UnmanagedTypeField.GetValue(null);
                var utf8PtrToStringMethod = typeof(Marshal).GetMethod
                (
                    "PtrToStringUTF8",
                    new[] { typeof(IntPtr) }
                );

                _ptrToStringMethods.Add
                (
                    _utf8UnmanagedType.Value,
                    utf8PtrToStringMethod
                );

                var utf8StringToPtrMethod = typeof(Marshal).GetMethod
                (
                    "StringToCoTaskMemUTF8",
                    new[] { typeof(string) }
                );

                _stringToPtrMethods.Add
                (
                    _utf8UnmanagedType.Value,
                    utf8StringToPtrMethod
                );
            }

            // Memory freeing methods
            _freeBStrMethod = typeof(Marshal).GetMethod
            (
                nameof(Marshal.FreeBSTR),
                new[] { typeof(IntPtr) }
            )
            ?? throw new MethodNotFoundException(nameof(Marshal.FreeBSTR));

            _freeHGlobalMethod = typeof(Marshal).GetMethod
            (
                nameof(Marshal.FreeHGlobal),
                new[] { typeof(IntPtr) }
            )
            ?? throw new MethodNotFoundException(nameof(Marshal.FreeHGlobal));

            _freeCoTaskMemMethod = typeof(Marshal).GetMethod
            (
                nameof(Marshal.FreeCoTaskMem),
                new[] { typeof(IntPtr) }
            )
            ?? throw new MethodNotFoundException(nameof(Marshal.FreeCoTaskMem));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StringMarshallingWrapper"/> class.
        /// </summary>
        /// <param name="targetModule">The module where the implementation should be generated.</param>
        /// <param name="targetType">The type in which the implementation should be generated.</param>
        /// <param name="targetTypeConstructorIL">The IL generator for the target type's constructor.</param>
        /// <param name="options">The configuration object to use.</param>
        public StringMarshallingWrapper
        (
            [NotNull] ModuleBuilder targetModule,
            [NotNull] TypeBuilder targetType,
            [NotNull] ILGenerator targetTypeConstructorIL,
            ImplementationOptions options
        )
            : base
            (
                targetModule,
                targetType,
                targetTypeConstructorIL,
                options
            )
        {
        }

        /// <inheritdoc />
        public override bool IsApplicable(IntrospectiveMethodInfo method)
        {
            var hasAnyStringParameters = method.ReturnType == typeof(string) ||
                                         method.ParameterTypes.Any(t => t == typeof(string));

            return hasAnyStringParameters;
        }

        /// <inheritdoc />
        public override void EmitPrologue
        (
            ILGenerator il,
            PipelineWorkUnit<IntrospectiveMethodInfo> workUnit
        )
        {
            var definition = workUnit.Definition;

            var locals = new Dictionary<int, LocalBuilder>();
            _workUnitLocals.Add(workUnit, locals);

            // Load the "this" reference
            il.EmitLoadArgument(0);

            for (short i = 1; i <= definition.ParameterTypes.Count; ++i)
            {
                il.EmitLoadArgument(i);

                var parameterType = definition.ParameterTypes[i - 1];
                if (parameterType != typeof(string))
                {
                    continue;
                }

                var unmanagedStringType = GetParameterUnmanagedType(definition.ParameterCustomAttributes[i - 1]);
                il.EmitCallDirect(SelectManagedToUnmanagedTransformationMethod(unmanagedStringType));

                if (definition.ParameterHasCustomAttribute<CallerFreeAttribute>(i - 1))
                {
                    var parameterLocal = il.DeclareLocal(typeof(IntPtr));
                    il.EmitSetLocalVariable(parameterLocal);
                    il.EmitLoadLocalVariable(parameterLocal);

                    locals.Add(i - 1, parameterLocal);
                }
            }
        }

        /// <inheritdoc />
        public override void EmitEpilogue
        (
            ILGenerator il,
            PipelineWorkUnit<IntrospectiveMethodInfo> workUnit
        )
        {
            var definition = workUnit.Definition;

            var locals = _workUnitLocals[workUnit];
            if (locals.Any())
            {
                // We have cleanup to do (freeing unmanaged string memory)
                foreach (var localCombo in locals)
                {
                    var parameterIndex = localCombo.Key;
                    var local = localCombo.Value;

                    var unmanagedStringType = GetParameterUnmanagedType
                    (
                        definition.ParameterCustomAttributes[parameterIndex]
                    );

                    il.EmitLoadLocalVariable(local);
                    il.EmitCallDirect(SelectUnmanagedFreeMethod(unmanagedStringType));
                }

                _workUnitLocals.Remove(workUnit);
            }

            if (definition.ReturnType != typeof(string))
            {
                return;
            }

            var unmanagedReturnStringType = GetParameterUnmanagedType(definition.ReturnParameterCustomAttributes);
            if (definition.ReturnParameterHasCustomAttribute<CallerFreeAttribute>())
            {
                var ptrLocal = il.DeclareLocal(typeof(IntPtr));
                var returnLocal = il.DeclareLocal(typeof(string));

                // Store the pointer returned from native code
                il.EmitSetLocalVariable(ptrLocal);

                // Marshal the string from the pointer, and store it
                il.EmitLoadLocalVariable(ptrLocal);
                il.EmitCallDirect(SelectUnmanagedToManagedTransformationMethod(unmanagedReturnStringType));
                il.EmitSetLocalVariable(returnLocal);

                // Free the pointer
                il.EmitLoadLocalVariable(ptrLocal);
                il.EmitCallDirect(SelectUnmanagedFreeMethod(unmanagedReturnStringType));

                // Load the string
                il.EmitLoadLocalVariable(returnLocal);
            }
            else
            {
                il.EmitCallDirect(SelectUnmanagedToManagedTransformationMethod(unmanagedReturnStringType));
            }
        }

        /// <inheritdoc />
        public override IntrospectiveMethodInfo GeneratePassthroughDefinition
        (
            PipelineWorkUnit<IntrospectiveMethodInfo> workUnit
        )
        {
            var definition = workUnit.Definition;

            var newParameterTypes = definition.ParameterTypes.Select
            (
                t => t == typeof(string)
                    ? typeof(IntPtr)
                    : t
            ).ToArray();

            var newReturnType = definition.ReturnType == typeof(string) ? typeof(IntPtr) : definition.ReturnType;

            var passthroughMethod = TargetType.DefineMethod
            (
                $"{workUnit.GetUniqueBaseMemberName()}_wrapped",
                MethodAttributes.Private | MethodAttributes.Virtual | MethodAttributes.HideBySig,
                newReturnType,
                newParameterTypes
            );

            // Copy over all the attributes, except MarshalAsAttributes to IntPtr parameters
            passthroughMethod.ApplyCustomAttributesFrom
            (
                definition,
                newReturnType,
                newParameterTypes
            );

            return new IntrospectiveMethodInfo
            (
                passthroughMethod,
                newReturnType,
                newParameterTypes,
                definition.MetadataType,
                definition
            );
        }

        /// <summary>
        /// Selects the appropriate method to transforme a string value on the evaluation stack to an
        /// <see cref="IntPtr"/> of the given unmanaged type.
        /// </summary>
        /// <param name="unmanagedType">The unmanaged string type.</param>
        /// <returns>The method.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if the given unmanaged type is not a string type.
        /// </exception>
        [NotNull, Pure]
        private MethodInfo SelectManagedToUnmanagedTransformationMethod(UnmanagedType unmanagedType)
        {
            switch (unmanagedType)
            {
                case BStr:
                case LPStr:
                case LPWStr:
                {
                    return _stringToPtrMethods[unmanagedType];
                }
                case LPTStr:
                {
                    if (RuntimeInformation.FrameworkDescription.Contains("Mono"))
                    {
                        // Mono uses ANSI for Auto, but ANSI is no longer a supported charset. Use Unicode.
                        return _stringToPtrMethods[LPWStr];
                    }

                    // Use automatic selection
                    return _stringToPtrMethods[LPTStr];
                }
                default:
                {
                    if (!(_utf8UnmanagedType is null) && unmanagedType == _utf8UnmanagedType)
                    {
                        return _stringToPtrMethods[_utf8UnmanagedType.Value];
                    }

                    throw new ArgumentOutOfRangeException
                    (
                        nameof(unmanagedType),
                        "The unmanaged type wasn't a recognized string type."
                    );
                }
            }
        }

        /// <summary>
        /// Selects the appropriate method to transform a <see cref="IntPtr"/> value of the given unmanaged type on the
        /// evaluation stack to a managed string.
        /// </summary>
        /// <param name="unmanagedType">The unmanaged string type.</param>
        /// <returns>The method.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if the given unmanaged type is not a string type.
        /// </exception>
        [NotNull, Pure]
        private MethodInfo SelectUnmanagedToManagedTransformationMethod(UnmanagedType unmanagedType)
        {
            switch (unmanagedType)
            {
                case BStr:
                case LPStr:
                case LPWStr:
                {
                    return _ptrToStringMethods[unmanagedType];
                }
                case LPTStr:
                {
                    if (RuntimeInformation.FrameworkDescription.Contains("Mono"))
                    {
                        // Mono uses ANSI for Auto, but ANSI is no longer a supported charset. Use Unicode.
                        return _ptrToStringMethods[LPWStr];
                    }

                    // Use automatic selection
                    return _ptrToStringMethods[LPTStr];
                }
                default:
                {
                    if (!(_utf8UnmanagedType is null) && unmanagedType == _utf8UnmanagedType)
                    {
                        return _ptrToStringMethods[_utf8UnmanagedType.Value];
                    }

                    throw new ArgumentOutOfRangeException
                    (
                        nameof(unmanagedType),
                        "The unmanaged type wasn't a recognized string type."
                    );
                }
            }
        }

        /// <summary>
        /// Selects the appropriate method to free an <see cref="IntPtr"/> to an unmanaged string on the evaluation
        /// stack.
        /// </summary>
        /// <param name="unmanagedType">The unmanaged string type.</param>
        /// <returns>The method.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if the given unmanaged type is not a string type.
        /// </exception>
        [NotNull, Pure]
        private MethodInfo SelectUnmanagedFreeMethod(UnmanagedType unmanagedType)
        {
            switch (unmanagedType)
            {
                case BStr:
                {
                    return _freeBStrMethod;
                }
                case LPStr:
                case LPTStr:
                case LPWStr:
                {
                    return _freeHGlobalMethod;
                }
                default:
                {
                    if (!(_utf8UnmanagedType is null) && unmanagedType == _utf8UnmanagedType)
                    {
                        return _freeCoTaskMemMethod;
                    }

                    throw new ArgumentOutOfRangeException
                    (
                        nameof(unmanagedType),
                        "The unmanaged type wasn't a recognized string type."
                    );
                }
            }
        }

        /// <summary>
        /// Gets the unmanaged type that the parameter with the given attributes should be marshalled as. The return
        /// type is guaranteed to be one of the string types. If no type is specified, a LPTStr is assumed.
        /// </summary>
        /// <param name="customAttributes">The custom attributes applied to the parameter.</param>
        /// <returns>The parameter type.</returns>
        private UnmanagedType GetParameterUnmanagedType
        (
            [NotNull, ItemNotNull] IEnumerable<CustomAttributeData> customAttributes
        )
        {
            var marshalAsAttribute = customAttributes.FirstOrDefault
            (
                a =>
                    a.AttributeType == typeof(MarshalAsAttribute)
            );

            if (marshalAsAttribute is null)
            {
                // Default to marshalling strings as ansi strings
                return LPStr;
            }

            return marshalAsAttribute.ToInstance<MarshalAsAttribute>().Value;
        }
    }
}
