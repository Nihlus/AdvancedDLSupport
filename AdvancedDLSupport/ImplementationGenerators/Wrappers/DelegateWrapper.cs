//
//  DelegateWrapper.cs
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
using StrictEmit;
using static AdvancedDLSupport.ImplementationGenerators.GeneratorComplexity;

namespace AdvancedDLSupport.ImplementationGenerators
{
    /// <summary>
    /// Generates wrapper instructions for marshalling delegate type.
    /// </summary>
    public class DelegateWrapper : CallWrapperBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DelegateWrapper"/> class.
        /// </summary>
        /// <param name="targetModule">The module where the implementation should be generated.</param>
        /// <param name="targetType">The type in which the implementation should be generated.</param>
        /// <param name="targetTypeConstructorIL">The IL generator for the target type's constructor.</param>
        /// <param name="options">The configuration object to use.</param>
        public DelegateWrapper([NotNull] ModuleBuilder targetModule, [NotNull] TypeBuilder targetType, [NotNull] ILGenerator targetTypeConstructorIL, ImplementationOptions options)
            : base(targetModule, targetType, targetTypeConstructorIL, options)
        {
        }

        /// <inheritdoc/>
        public override GeneratorComplexity Complexity => MemberDependent | TransformsParameters | CreatesTypes;

        /// <inheritdoc />
        public override bool IsApplicable(IntrospectiveMethodInfo member)
        {
            if (member.ReturnType.IsDelegate() && GetParameterDelegateLifetime(member.ReturnParameterCustomAttributes) == DelegateLifetime.Persistent)
            {
                return true;
            }

            for (int i = 0; i < member.ParameterTypes.Count; i++)
            {
                var p = member.ParameterTypes[i];
                if (p.IsDelegate() && GetParameterDelegateLifetime(member.ParameterCustomAttributes[i]) == DelegateLifetime.Persistent)
                {
                    return true;
                }
            }

            return false;
        }

        /// <inheritdoc/>
        public override void EmitPrologue(ILGenerator il, PipelineWorkUnit<IntrospectiveMethodInfo> workUnit)
        {
            var definition = workUnit.Definition;

            var allocMethod =
                typeof(NativeLibraryBase).GetMethod("AddLifetimeDelegate", BindingFlags.Instance | BindingFlags.NonPublic);
            il.EmitLoadArgument(0);
            for (short i = 1; i <= definition.ParameterTypes.Count; ++i)
            {
                var parameterType = definition.ParameterTypes[i - 1];

                if (!parameterType.IsDelegate())
                {
                    il.EmitLoadArgument(i);
                    continue;
                }

                var lifetime = GetParameterDelegateLifetime(definition.ParameterCustomAttributes[i - 1]);

                if (lifetime == DelegateLifetime.Persistent)
                {
                    // Load this
                    il.EmitLoadArgument(0);
                    il.EmitLoadArgument(i);
                    il.Emit(OpCodes.Call, allocMethod);
                }
                else
                {
                    il.EmitLoadArgument(i);
                }
            }
        }

        /// <inheritdoc/>
        public override void EmitEpilogue(ILGenerator il, PipelineWorkUnit<IntrospectiveMethodInfo> workUnit)
        {
            // If the return type is a delegate, convert it back into its generic representation
            var definition = workUnit.Definition;
            var returnType = definition.ReturnType;

            if (!returnType.IsDelegate())
            {
                return;
            }

            var marshalPointerToDel = typeof(Marshal).GetMethods(BindingFlags.Public | BindingFlags.Static).FirstOrDefault(x =>
                x.IsGenericMethodDefinition && x.Name == "GetDelegateForFunctionPointer" && x.GetParameters().Length == 1 && x.GetParameters()[0].ParameterType == typeof(IntPtr));

            if (marshalPointerToDel == null)
            {
                throw new InvalidOperationException("Marshal.GetDelegateForFunctionPointer<T>(IntPtr) not found");
            }

            var intPtrEquality = typeof(IntPtr).GetMethod("op_Equality");
            var intPtrZero = typeof(IntPtr).GetField("Zero");

            var retNullLabel = il.DefineLabel();
            var intPtrRetVal = il.DeclareLocal(typeof(IntPtr));

            il.Emit(OpCodes.Stloc_0);
            il.Emit(OpCodes.Ldloc_0);

            il.Emit(OpCodes.Ldsfld, intPtrZero);
            il.Emit(OpCodes.Call, intPtrEquality);

            il.Emit(OpCodes.Brtrue_S, retNullLabel);

            il.Emit(OpCodes.Ldloc_0);

            marshalPointerToDel = marshalPointerToDel.MakeGenericMethod(workUnit.Definition.ReturnType);

            il.Emit(OpCodes.Call, marshalPointerToDel);
            il.Emit(OpCodes.Ret);

            il.MarkLabel(retNullLabel);

            il.Emit(OpCodes.Ldnull);
        }

        /// <summary>
        /// Gets the delegate lifetime that the parameter with the given attributes should be marshalled with.
        /// </summary>
        /// <param name="customAttributes">The custom attributes applied to the parameter.</param>
        /// <returns>The delegate lifetime.</returns>
        [Pure]
        private DelegateLifetime GetParameterDelegateLifetime([NotNull, ItemNotNull] IEnumerable<CustomAttributeData> customAttributes)
        {
            var lifetimeAttribute = customAttributes.FirstOrDefault
            (
                a =>
                    a.AttributeType == typeof(DelegateLifetimeAttribute)
            );

            if (lifetimeAttribute is null)
            {
                // Default to keeping delegates alive
                return DelegateLifetime.Persistent;
            }

            return lifetimeAttribute.ToInstance<DelegateLifetimeAttribute>().Lifetime;
        }

        /// <inheritdoc/>
        public override IntrospectiveMethodInfo GeneratePassthroughDefinition
        (
            PipelineWorkUnit<IntrospectiveMethodInfo> workUnit
        )
        {
            var definition = workUnit.Definition;

            var newReturnType = GetParameterPassthroughType(definition.ReturnType, GetParameterDelegateLifetime(definition.ReturnParameterCustomAttributes));
            var newParameterTypes = definition.ParameterTypes.Select
            (
                (t, i) => GetParameterPassthroughType(t, GetParameterDelegateLifetime(definition.ParameterCustomAttributes[i]))
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
        /// Gets the type that the parameter type should be passed through as.
        /// </summary>
        /// <param name="originalType">The original type.</param>
        /// <returns>The passed-through type.</returns>
        [NotNull]
        private Type GetParameterPassthroughType([NotNull] Type originalType, DelegateLifetime lifetime)
        {
            if (originalType.IsDelegate() && lifetime == DelegateLifetime.Persistent)
            {
                return typeof(IntPtr);
            }

            return originalType;
        }
    }
}
