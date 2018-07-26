//
//  GenericDelegateWrapper.cs
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
using System.Text;
using AdvancedDLSupport.Extensions;
using AdvancedDLSupport.Pipeline;
using AdvancedDLSupport.Reflection;
using JetBrains.Annotations;
using Mono.DllMap.Extensions;
using static AdvancedDLSupport.ImplementationGenerators.GeneratorComplexity;

namespace AdvancedDLSupport.ImplementationGenerators
{
    /// <summary>
    /// Generates wrapper instructions for marshalling generic delegate types (<see cref="Func{T}"/>,
    /// <see cref="Action{T}"/> and their variants.)
    /// </summary>
    internal sealed class GenericDelegateWrapper : CallWrapperBase
    {
        /// <inheritdoc/>
        public override GeneratorComplexity Complexity => MemberDependent | TransformsParameters | CreatesTypes;

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericDelegateWrapper"/> class.
        /// </summary>
        /// <param name="targetModule">The module where the implementation should be generated.</param>
        /// <param name="targetType">The type in which the implementation should be generated.</param>
        /// <param name="targetTypeConstructorIL">The IL generator for the target type's constructor.</param>
        /// <param name="options">The configuration object to use.</param>
        public GenericDelegateWrapper
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

        /// <inheritdoc/>
        public override bool IsApplicable(IntrospectiveMethodInfo member)
        {
            if (member.ReturnType.IsGenericDelegate())
            {
                return true;
            }

            return member.ParameterTypes.Any(t => t.IsGenericDelegate());
        }

        /// <inheritdoc/>
        public override void EmitAdditionalTypes
        (
            ModuleBuilder module,
            PipelineWorkUnit<IntrospectiveMethodInfo> workUnit
        )
        {
            var definition = workUnit.Definition;

            foreach (var parameterType in definition.ParameterTypes.Concat(new[] { definition.ReturnType }))
            {
                if (!parameterType.IsGenericDelegate())
                {
                    continue;
                }

                var signature = GetSignatureTypesFromGenericDelegate(parameterType);
                var delegateName = GetDelegateTypeName(signature.ReturnType, signature.ParameterTypes);

                module.DefineDelegate
                (
                    delegateName,
                    CallingConvention.Cdecl,
                    signature.ReturnType,
                    signature.ParameterTypes.ToArray(),
                    Options.HasFlagFast(ImplementationOptions.SuppressSecurity)
                );
            }
        }

        /// <inheritdoc/>
        public override void EmitPrologue(ILGenerator il, PipelineWorkUnit<IntrospectiveMethodInfo> workUnit)
        {
            // Construct explict delegates from generic ones
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public override void EmitEpilogue(ILGenerator il, PipelineWorkUnit<IntrospectiveMethodInfo> workUnit)
        {
            // If the return type is a delegate, convert it back into its generic representation
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public override IntrospectiveMethodInfo GeneratePassthroughDefinition
        (
            PipelineWorkUnit<IntrospectiveMethodInfo> workUnit
        )
        {
            var definition = workUnit.Definition;

            var newReturnType = GetParameterPassthroughType(definition.ReturnType);
            var newParameterTypes = definition.ParameterTypes.Select(GetParameterPassthroughType).ToArray();

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
        /// Gets the type that the parameter type should be passed through as.
        /// </summary>
        /// <param name="originalType">The original type.</param>
        /// <returns>The passed-through type.</returns>
        [NotNull]
        private Type GetParameterPassthroughType([NotNull] Type originalType)
        {
            if (originalType.IsGenericDelegate())
            {
                var signature = GetSignatureTypesFromGenericDelegate(originalType);
                var delegateName = GetDelegateTypeName(signature.ReturnType, signature.ParameterTypes);

                return TargetModule.GetType(delegateName);
            }

            return originalType;
        }

        /// <summary>
        /// Gets a method signature from the given generic delegate, consisting of a return type and parameter types.
        /// </summary>
        /// <param name="delegateType">The type to inspect.</param>
        /// <returns>The types.</returns>
        /// <exception cref="InvalidOperationException">Thrown if no types could be extracted.</exception>
        private (Type ReturnType, IReadOnlyList<Type> ParameterTypes) GetSignatureTypesFromGenericDelegate
        (
            [NotNull] Type delegateType
        )
        {
            var typeParameters = delegateType.GenericTypeArguments;

            if (delegateType.IsGenericFuncDelegate())
            {
                if (typeParameters.Length == 1)
                {
                    return (typeParameters[0], new List<Type>());
                }

                return (typeParameters.Last(), typeParameters.Take(typeParameters.Length - 1).ToList());
            }

            if (delegateType.IsGenericActionDelegate())
            {
                return (typeof(void), typeParameters);
            }

            throw new InvalidOperationException("Couldn't extract a method signature from the type.");
        }

        /// <summary>
        /// Gets the generated name for an explicit delegate implementation that returns the given type and takes the
        /// given parameters. The name is guaranteed to be identical given the same input types in the same order.
        /// </summary>
        /// <param name="returnType">The return type of the delegate.</param>
        /// <param name="parameterTypes">The parameter types of the delegate.</param>
        /// <returns>The generated name of the delegate.</returns>
        [NotNull]
        private string GetDelegateTypeName([NotNull] Type returnType, [NotNull] IEnumerable<Type> parameterTypes)
        {
            var sb = new StringBuilder();

            sb.Append("generic_delegate_implementation_");
            sb.Append($"r{returnType.Name}_");
            sb.Append($"p{string.Join("_p", parameterTypes)}");

            return sb.ToString();
        }
    }
}
