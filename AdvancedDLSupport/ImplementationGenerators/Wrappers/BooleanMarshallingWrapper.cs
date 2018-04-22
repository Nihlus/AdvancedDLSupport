//
//  BooleanMarshallingWrapper.cs
//
//  Copyright (c) 2018 Firwood Software
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
using Mono.DllMap.Extensions;
using StrictEmit;

using static AdvancedDLSupport.ImplementationOptions;
using static System.Runtime.InteropServices.UnmanagedType;

#pragma warning disable SA1513

namespace AdvancedDLSupport.ImplementationGenerators
{
    /// <summary>
    /// Generates wrapper instructions for marshalling boolean parameters under indirect calling conditions, where
    /// normal marshalling is not available.
    /// </summary>
    public class BooleanMarshallingWrapper : CallWrapperBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BooleanMarshallingWrapper"/> class.
        /// </summary>
        /// <param name="targetModule">The module where the implementation should be generated.</param>
        /// <param name="targetType">The type in which the implementation should be generated.</param>
        /// <param name="targetTypeConstructorIL">The IL generator for the target type's constructor.</param>
        /// <param name="options">The configuration object to use.</param>
        public BooleanMarshallingWrapper
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
            var hasAnyBooleanParameters = method.ReturnType == typeof(bool) || method.ParameterTypes.Any(t => t == typeof(bool));

            return hasAnyBooleanParameters && Options.HasFlagFast(UseIndirectCalls);
        }

        /// <inheritdoc />
        public override void EmitPrologue(ILGenerator il, PipelineWorkUnit<IntrospectiveMethodInfo> workUnit)
        {
            var definition = workUnit.Definition;

            // Load the "this" reference
            il.EmitLoadArgument(0);

            for (short i = 1; i <= definition.ParameterTypes.Count; ++i)
            {
                il.EmitLoadArgument(i);

                var parameterType = definition.ParameterTypes[i - 1];
                if (parameterType != typeof(bool))
                {
                    continue;
                }

                // Convert the input boolean to an unmanaged integer
                var unmanagedType = GetParameterUnmanagedType(definition.ParameterCustomAttributes[i]);
                EmitBooleanToUnmanagedIntegerConversion(il, unmanagedType);
            }
        }

        /// <inheritdoc />
        public override void EmitEpilogue(ILGenerator il, PipelineWorkUnit<IntrospectiveMethodInfo> workUnit)
        {
            var definition = workUnit.Definition;

            if (definition.ReturnType != typeof(bool))
            {
                return;
            }

            var unmanagedType = GetParameterUnmanagedType(definition.ReturnParameterCustomAttributes);
            EmitUnmanagedIntegerToBooleanConversion(il, unmanagedType);
        }

        /// <inheritdoc />
        public override IntrospectiveMethodInfo GeneratePassthroughDefinition(PipelineWorkUnit<IntrospectiveMethodInfo> workUnit)
        {
            var definition = workUnit.Definition;

            var newReturnType = GetParameterMarshallingType(definition.ReturnParameterCustomAttributes);
            var newParameterTypes = definition.ParameterTypes.Select
            (
                (parameterType, i) =>
                    parameterType == typeof(bool)
                        ? GetParameterMarshallingType(definition.ParameterCustomAttributes[i])
                        : parameterType
            ).ToArray();

            var passthroughMethod = TargetType.DefineMethod
            (
                $"{workUnit.GetUniqueBaseMemberName()}_wrapped",
                MethodAttributes.Private | MethodAttributes.Virtual | MethodAttributes.HideBySig,
                CallingConventions.Standard,
                newReturnType,
                newParameterTypes
            );

            passthroughMethod.ApplyCustomAttributesFrom(definition, newReturnType, newParameterTypes);

            return new IntrospectiveMethodInfo(passthroughMethod, newReturnType, newParameterTypes, definition);
        }

        /// <summary>
        /// Emits the IL instructions neccesary to convert a boolean value on the evaluation stack to its unmanaged
        /// integer representation.
        /// </summary>
        /// <param name="il">The generator where the IL is to be emitted.</param>
        /// <param name="unmanagedType">The unmanaged type of the boolean.</param>
        private void EmitBooleanToUnmanagedIntegerConversion([NotNull] ILGenerator il, UnmanagedType unmanagedType)
        {
            var trueCase = il.DefineLabel();
            var endOfCondition = il.DefineLabel();

            il.EmitBranchTrue(trueCase);

            // false case
            if (unmanagedType == U8 || unmanagedType == I8)
            {
                il.EmitConstantLong(GetBooleanIntegerValueForUnmanagedType(unmanagedType, false));
            }
            else
            {
                il.EmitConstantInt((int)GetBooleanIntegerValueForUnmanagedType(unmanagedType, false));
            }

            il.EmitBranch(endOfCondition);

            // true case
            il.MarkLabel(trueCase);
            if (unmanagedType == U8 || unmanagedType == I8)
            {
                il.EmitConstantLong(GetBooleanIntegerValueForUnmanagedType(unmanagedType, true));
            }
            else
            {
                il.EmitConstantInt((int)GetBooleanIntegerValueForUnmanagedType(unmanagedType, true));
            }

            il.MarkLabel(endOfCondition);
        }

        /// <summary>
        /// Emits the IL instructions neccesary to convert an unmanaged integer value on the evaluation stack to its
        /// boolean representation.
        /// </summary>
        /// <param name="il">The generator where the IL is to be emitted.</param>
        /// <param name="unmanagedType">The unmanaged type of the boolean.</param>
        private void EmitUnmanagedIntegerToBooleanConversion([NotNull] ILGenerator il, UnmanagedType unmanagedType)
        {
            var trueCase = il.DefineLabel();
            var endOfCondition = il.DefineLabel();

            // Convert whatever's on the stack to a long
            il.EmitConvertToLong();

            // Push the value for true onto the stack
            il.EmitConstantLong(GetBooleanIntegerValueForUnmanagedType(unmanagedType, true));
            il.EmitBranchIfEqual(trueCase);

            // false case
            il.EmitConstantInt(0);
            il.EmitBranch(endOfCondition);

            // true case
            il.MarkLabel(trueCase);
            il.EmitConstantInt(1);

            il.MarkLabel(endOfCondition);
        }

        /// <summary>
        /// Converts a boolean into its integer representation, based on the given unmanaged type.
        /// </summary>
        /// <param name="unmanagedType">The unmanaged type.</param>
        /// <param name="value">The boolean value.</param>
        /// <returns>The integer representation.</returns>
        private long GetBooleanIntegerValueForUnmanagedType(UnmanagedType unmanagedType, bool value)
        {
            switch (unmanagedType)
            {
                case I1:
                case I2:
                case I4:
                case I8:
                case U1:
                case U2:
                case U4:
                case U8:
                case Bool:
                {
                    return value ? 1 : 0;
                }
                case VariantBool:
                {
                    return value ? -1 : 0;
                }
                default:
                {
                    return value ? 1 : 0;
                }
            }
        }

        /// <summary>
        /// Gets the unmanaged type that the parameter with the given attributes should be marshalled as. The return
        /// type is guaranteed to be one of the signed or unsigned integer types. If no type is specified, a 1-byte
        /// unsigned integer is assumed.
        /// </summary>
        /// <param name="customAttributes">The custom attributes applied to the parameter.</param>
        /// <returns>The parameter type.</returns>
        private UnmanagedType GetParameterUnmanagedType([NotNull, ItemNotNull] IEnumerable<CustomAttributeData> customAttributes)
        {
            var marshalAsAttribute = customAttributes.FirstOrDefault
            (
                a =>
                    a.AttributeType == typeof(MarshalAsAttribute)
            );

            if (marshalAsAttribute is null)
            {
                // Default to marshalling booleans as 1-byte integers
                return U1;
            }

            return marshalAsAttribute.ToInstance<MarshalAsAttribute>().Value;
        }

        /// <summary>
        /// Gets the type that the parameter with the given attributes should be marshalled as. The return type is
        /// guaranteed to be one of the signed or unsigned integer types. If no paramter type is specified, a 1-byte
        /// unsigned integer is assumed.
        /// </summary>
        /// <param name="customAttributes">The custom attributes applied to the parameter.</param>
        /// <returns>The parameter type.</returns>
        [NotNull]
        private Type GetParameterMarshallingType([NotNull, ItemNotNull] IEnumerable<CustomAttributeData> customAttributes)
        {
            var unmanagedType = GetParameterUnmanagedType(customAttributes);

            switch (unmanagedType)
            {
                case I1:
                {
                    return typeof(sbyte);
                }
                case I2:
                case VariantBool:
                {
                    return typeof(short);
                }
                case I4:
                case Bool:
                {
                    return typeof(int);
                }
                case I8:
                {
                    return typeof(long);
                }
                case U1:
                {
                    return typeof(byte);
                }
                case U2:
                {
                    return typeof(ushort);
                }
                case U4:
                {
                    return typeof(uint);
                }
                case U8:
                {
                    return typeof(ulong);
                }
                default:
                {
                    return typeof(byte);
                }
            }
        }
    }
}
