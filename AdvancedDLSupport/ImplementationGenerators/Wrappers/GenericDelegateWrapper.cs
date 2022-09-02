//
//  GenericDelegateWrapper.cs
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
using System.Text;
using AdvancedDLSupport.Extensions;
using AdvancedDLSupport.Pipeline;
using AdvancedDLSupport.Reflection;
using Mono.DllMap.Extensions;
using StrictEmit;
using static AdvancedDLSupport.ImplementationGenerators.GeneratorComplexity;

namespace AdvancedDLSupport.ImplementationGenerators;

/// <summary>
/// Generates wrapper instructions for marshalling generic delegate types (<see cref="Func{T}"/>,
/// <see cref="Action{T}"/> and their variants).
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
        ModuleBuilder targetModule,
        TypeBuilder targetType,
        ILGenerator targetTypeConstructorIL,
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

            EmitExplicitDelegateDefinition(module, parameterType);
        }
    }

    /// <summary>
    /// Generates an explicit delegate definition based on a generic delegate type.
    /// </summary>
    /// <param name="module">The module to emit the type in.</param>
    /// <param name="genericDelegateType">The generic delegate type.</param>
    private TypeInfo EmitExplicitDelegateDefinition(ModuleBuilder module, Type genericDelegateType)
    {
        var existingDelegate = GetCreatedExplicitDelegateType(genericDelegateType);
        if (!(existingDelegate is null))
        {
            return existingDelegate.GetTypeInfo();
        }

        var signature = GetSignatureTypesFromGenericDelegate(genericDelegateType);

        var delegateReturnType = signature.ReturnType;
        if (delegateReturnType.IsGenericDelegate())
        {
            // This is a nested delegate, so we'll need to generate one for this one
            delegateReturnType = EmitExplicitDelegateDefinition(module, signature.ReturnType);
        }

        var delegateParameters = new List<TypeInfo>();
        foreach (var delegateParameter in signature.ParameterTypes)
        {
            if (!delegateParameter.IsGenericDelegate())
            {
                delegateParameters.Add(delegateParameter.GetTypeInfo());
                continue;
            }

            // Also a nested delegate, so we'll need to generate one for this one too
            var nestedDelegate = EmitExplicitDelegateDefinition(module, delegateParameter);
            delegateParameters.Add(nestedDelegate);
        }

        if (delegateParameters.Any(p => p.IsGenericDelegate()))
        {
            // break
        }

        var delegateName = GetDelegateTypeName(delegateReturnType, delegateParameters);
        var delegateDefinition = module.DefineDelegate
        (
            delegateName,
            CallingConvention.Cdecl,
            delegateReturnType,
            delegateParameters.Cast<Type>().ToArray(),
            Options.HasFlagFast(ImplementationOptions.SuppressSecurity)
        );

        return delegateDefinition.CreateTypeInfo()!;
    }

    /// <inheritdoc/>
    public override void EmitPrologue(ILGenerator il, PipelineWorkUnit<IntrospectiveMethodInfo> workUnit)
    {
        var definition = workUnit.Definition;

        // Load the "this" reference
        il.EmitLoadArgument(0);

        for (short i = 1; i <= definition.ParameterTypes.Count; ++i)
        {
            il.EmitLoadArgument(i);

            var parameterType = definition.ParameterTypes[i - 1];
            if (!parameterType.IsGenericDelegate())
            {
                continue;
            }

            // Convert the input generic delegate to an explicit delegate
            var explicitDelegateType = GetCreatedExplicitDelegateType(parameterType);

            if (explicitDelegateType is null)
            {
                throw new InvalidOperationException("No delegate type has been created for the given type.");
            }

            var explicitDelegateConstructor = explicitDelegateType.GetConstructors().First();
            var invokeMethod = parameterType.GetMethod("Invoke");

            il.EmitLoadFunctionPointer(invokeMethod);
            il.EmitNewObject(explicitDelegateConstructor);
        }
    }

    /// <inheritdoc/>
    public override void EmitEpilogue(ILGenerator il, PipelineWorkUnit<IntrospectiveMethodInfo> workUnit)
    {
        // If the return type is a delegate, convert it back into its generic representation
        var definition = workUnit.Definition;
        var returnType = definition.ReturnType;

        if (!returnType.IsGenericDelegate())
        {
            return;
        }

        // Convert the output explicit delegate to a generic delegate
        var explicitDelegateType = GetCreatedExplicitDelegateType(returnType);

        if (explicitDelegateType is null)
        {
            throw new InvalidOperationException("No delegate type has been created for the given type.");
        }

        var genericDelegateConstructor = returnType.GetConstructors().First();
        var invokeMethod = explicitDelegateType.GetMethod("Invoke");

        il.EmitLoadFunctionPointer(invokeMethod);
        il.EmitNewObject(genericDelegateConstructor);
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
    private Type GetParameterPassthroughType(Type originalType)
    {
        if (!originalType.IsGenericDelegate())
        {
            return originalType;
        }

        var explicitDelegateType = GetCreatedExplicitDelegateType(originalType);
        if (explicitDelegateType is null)
        {
            throw new InvalidOperationException
            (
                "Could not find the generated delegate type."
            );
        }

        return explicitDelegateType;
    }

    /// <summary>
    /// Gets an already created explicit delegate type, based on the original generic delegate type.
    /// </summary>
    /// <param name="originalType">The generic type.</param>
    /// <returns>The explicitly implemented type.</returns>
    private Type? GetCreatedExplicitDelegateType(Type originalType)
    {
        var signature = GetSignatureTypesFromGenericDelegate(originalType);
        var delegateName = GetDelegateTypeName(signature.ReturnType, signature.ParameterTypes);

        return TargetModule.GetType(delegateName);
    }

    /// <summary>
    /// Gets a method signature from the given generic delegate, consisting of a return type and parameter types.
    /// </summary>
    /// <param name="delegateType">The type to inspect.</param>
    /// <returns>The types.</returns>
    /// <exception cref="InvalidOperationException">Thrown if no types could be extracted.</exception>
    private (Type ReturnType, IReadOnlyList<Type> ParameterTypes) GetSignatureTypesFromGenericDelegate
    (
        Type delegateType
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
    private string GetDelegateTypeName(Type returnType, IReadOnlyCollection<Type> parameterTypes)
    {
        var sb = new StringBuilder();

        sb.Append("generic_delegate_implementation_");
        var returnTypeName = returnType.Name;
        if (returnType.IsGenericDelegate())
        {
            var signature = GetSignatureTypesFromGenericDelegate(returnType);
            returnTypeName = GetDelegateTypeName(signature.ReturnType, signature.ParameterTypes);
        }

        sb.Append($"r{returnTypeName}_");

        var parameterNames = new List<string>();
        foreach (var parameterType in parameterTypes)
        {
            var parameterName = parameterType.Name;
            if (parameterType.IsGenericDelegate())
            {
                var signature = GetSignatureTypesFromGenericDelegate(parameterType);
                parameterName = GetDelegateTypeName(signature.ReturnType, signature.ParameterTypes);
            }

            parameterNames.Add(parameterName);
        }

        sb.Append($"p{string.Join("_p", parameterNames)}");

        return sb.ToString();
    }
}
