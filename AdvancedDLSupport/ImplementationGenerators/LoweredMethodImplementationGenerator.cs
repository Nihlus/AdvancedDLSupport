//
//  LoweredMethodImplementationGenerator.cs
//
//  Copyright (c) 2018 Firwood Software
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using AdvancedDLSupport.Extensions;
using AdvancedDLSupport.Reflection;
using JetBrains.Annotations;
using Mono.DllMap.Extensions;

using static AdvancedDLSupport.ImplementationOptions;
using static System.Reflection.MethodAttributes;

namespace AdvancedDLSupport.ImplementationGenerators
{
    /// <summary>
    /// Generates method implementations for methods involving complex types.
    /// </summary>
    internal sealed class LoweredMethodImplementationGenerator : ImplementationGeneratorBase<IntrospectiveMethodInfo>
    {
        private readonly MethodImplementationGenerator _methodGenerator;

        [NotNull]
        private readonly TypeTransformerRepository _transformerRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="LoweredMethodImplementationGenerator"/> class.
        /// </summary>
        /// <param name="targetModule">The module in which the method implementation should be generated.</param>
        /// <param name="targetType">The type in which the method implementation should be generated.</param>
        /// <param name="targetTypeConstructorIL">The IL generator for the target type's constructor.</param>
        /// <param name="options">The configuration object to use.</param>
        /// <param name="transformerRepository">The repository where type transformers are stored.</param>
        public LoweredMethodImplementationGenerator
        (
            [NotNull] ModuleBuilder targetModule,
            [NotNull] TypeBuilder targetType,
            [NotNull] ILGenerator targetTypeConstructorIL,
            ImplementationOptions options,
            [NotNull] TypeTransformerRepository transformerRepository
        )
            : base(targetModule, targetType, targetTypeConstructorIL, options)
        {
            _transformerRepository = transformerRepository;
            _methodGenerator = new MethodImplementationGenerator(targetModule, targetType, targetTypeConstructorIL, options);
        }

        /// <inheritdoc />
        protected override void GenerateImplementation(IntrospectiveMethodInfo method, string symbolName, string uniqueMemberIdentifier)
        {
            var complexMethodDefinition = GenerateComplexMethodDefinition(method);

            var implementation = GenerateImplementationForDefinition(complexMethodDefinition, symbolName, uniqueMemberIdentifier);

            TargetType.DefineMethodOverride(implementation.GetWrappedMember(), method.GetWrappedMember());
        }

        /// <inheritdoc />
        public override IntrospectiveMethodInfo GenerateImplementationForDefinition(IntrospectiveMethodInfo complexMethodDefinition, string symbolName, string uniqueMemberIdentifier)
        {
            var loweredMethod = GenerateLoweredMethodDefinition(complexMethodDefinition, uniqueMemberIdentifier);
            _methodGenerator.GenerateImplementationForDefinition(loweredMethod, symbolName, uniqueMemberIdentifier);

            return GenerateComplexMethodBody(complexMethodDefinition, loweredMethod);
        }

        /// <summary>
        /// Generates a lowered method based on the given complex method, lowering its return value and parameters.
        /// </summary>
        /// <param name="complexInterfaceMethod">The complex interface method.</param>
        /// <param name="memberIdentifier">The unique member identifier to use.</param>
        /// <returns>(
        /// A <see cref="ValueTuple{T1, T2}"/>, containing the generated method builder and its paramete types.
        /// </returns>
        private IntrospectiveMethodInfo GenerateLoweredMethodDefinition
        (
            [NotNull] IntrospectiveMethodInfo complexInterfaceMethod,
            [NotNull] string memberIdentifier
        )
        {
            var newReturnType = LowerTypeIfRequired(complexInterfaceMethod.ReturnType);

            var newParameterTypes = new List<Type>();
            foreach (var parameterType in complexInterfaceMethod.ParameterTypes)
            {
                newParameterTypes.Add(LowerTypeIfRequired(parameterType));
            }

            var loweredMethodBuilder = TargetType.DefineMethod
            (
                $"{memberIdentifier}_lowered",
                Public | Final | Virtual | HideBySig | NewSlot,
                CallingConventions.Standard,
                newReturnType,
                newParameterTypes.ToArray()
            );

            return new IntrospectiveMethodInfo(loweredMethodBuilder, newReturnType, newParameterTypes, complexInterfaceMethod);
        }

        /// <summary>
        /// Generates the definition of the complex method.
        /// </summary>
        /// <param name="interfaceDefinition">The interface definition to base it on.</param>
        /// <returns>An introspective method info for the definition.</returns>
        private IntrospectiveMethodInfo GenerateComplexMethodDefinition([NotNull] IntrospectiveMethodInfo interfaceDefinition)
        {
            var methodBuilder = TargetType.DefineMethod
            (
                interfaceDefinition.Name,
                Public | Final | Virtual | HideBySig | NewSlot,
                CallingConventions.Standard,
                interfaceDefinition.ReturnType,
                interfaceDefinition.ParameterTypes.ToArray()
            );

            return new IntrospectiveMethodInfo(methodBuilder, interfaceDefinition.ReturnType, interfaceDefinition.ParameterTypes, interfaceDefinition);
        }

        /// <summary>
        /// Generates the method body for the complex method implementation. This method will lower all required
        /// arguments and call the lowered method, then raise the return value if required.
        /// </summary>
        /// <param name="complexDefinition">The complex method definition.</param>
        /// <param name="loweredMethod">The lowered method definition.</param>
        /// <returns>The generated invoker.</returns>
        private IntrospectiveMethodInfo GenerateComplexMethodBody
        (
            [NotNull] IntrospectiveMethodInfo complexDefinition,
            [NotNull] IntrospectiveMethodInfo loweredMethod
        )
        {
            if (!(complexDefinition.GetWrappedMember() is MethodBuilder builder))
            {
                throw new ArgumentNullException(nameof(complexDefinition), "Could not unwrap introspective method to method builder.");
            }

            var il = builder.GetILGenerator();

            if (Options.HasFlagFast(GenerateDisposalChecks))
            {
                EmitDisposalCheck(il);
            }

            var parameterTypes = complexDefinition.ParameterTypes;
            var loweredParameterTypes = loweredMethod.ParameterTypes;

            // Emit lowered parameters
            il.Emit(OpCodes.Ldarg_0);
            for (var i = 1; i <= parameterTypes.Count; ++i)
            {
                var parameterType = parameterTypes[i - 1];
                if (parameterType.RequiresLowering())
                {
                    var loweredParameterType = loweredParameterTypes[i - 1];
                    EmitParameterValueLowering(il, parameterType, loweredParameterType, i);
                }
                else
                {
                    il.Emit(OpCodes.Ldarg, i);
                }
            }

            // Call lowered method
            il.Emit(OpCodes.Call, loweredMethod.GetWrappedMember());

            // Emit return value raising
            if (complexDefinition.ReturnValueRequiresLowering())
            {
                EmitValueRaising(il, complexDefinition.ReturnType, loweredMethod.ReturnType);
            }

            il.Emit(OpCodes.Ret);

            return complexDefinition;
        }

        /// <summary>
        /// Emits a set of IL instructions which will load the argument at the given index and lower it using
        /// its corresponding <see cref="ITypeTransformer"/>, then place that value back onto the stack.
        /// </summary>
        /// <param name="il">The generator where the IL is to be emitted.</param>
        /// <param name="complexType">The complex type that the parameter starts off as.</param>
        /// <param name="simpleType">The simple type that the parameter is to be lowered to.</param>
        /// <param name="argumentIndex">The index of the parameter.</param>
        private void EmitParameterValueLowering
        (
            [NotNull] ILGenerator il,
            [NotNull] Type complexType,
            [NotNull] Type simpleType,
            int argumentIndex
        )
        {
            var transformerType = typeof(ITypeTransformer<,>).MakeGenericType(complexType, simpleType);
            var lowerValueFunc = transformerType.GetMethod(nameof(ITypeTransformer<object, object>.LowerValue));

            EmitGetTypeTransformerCall(il, complexType);

            il.Emit(OpCodes.Ldarg, argumentIndex); // Load the complex argument
            il.EmitGetCurrentMethodArgumentByIndex(argumentIndex); // Load the relevant parameter

            il.Emit(OpCodes.Callvirt, lowerValueFunc); // Lower it
        }

        /// <summary>
        /// Emits a set of IL instructions which will pop the topmost value from the evaluation stack, and raise its
        /// type using its corresponding <see cref="ITypeTransformer"/>, then place that value back onto the stack.
        /// </summary>
        /// <param name="il">The generator where the IL is to be emitted.</param>
        /// <param name="complexType">The complex type that the value should be raised to.</param>
        /// <param name="simpleType">The simple type that the value starts off as.</param>
        /// <param name="argumentIndex">
        /// The index of the argument that is being raised. Typically 0 for the return value.
        /// </param>
        private void EmitValueRaising
        (
            [NotNull] ILGenerator il,
            [NotNull] Type complexType,
            [NotNull] Type simpleType,
            int argumentIndex = 0
        )
        {
            var transformerType = typeof(ITypeTransformer<,>).MakeGenericType(complexType, simpleType);
            var raiseValueFunc = transformerType.GetMethod(nameof(ITypeTransformer<object, object>.RaiseValue));

            il.DeclareLocal(simpleType);
            il.Emit(OpCodes.Stloc_0); // Store the current value on the stack

            EmitGetTypeTransformerCall(il, complexType);

            il.Emit(OpCodes.Ldloc_0); // Load the result again
            il.EmitGetCurrentMethodArgumentByIndex(argumentIndex);

            il.Emit(OpCodes.Callvirt, raiseValueFunc); // Raise it
        }

        /// <summary>
        /// Emits a set of IL instructions which will retrieve the <see cref="ITypeTransformer"/> registered for the
        /// given complex type, placing it onto the evaluating stack.
        /// </summary>
        /// <param name="il">The generator where the IL is to be emitted.</param>
        /// <param name="complexType">The complex type which the transformer should handle.</param>
        private void EmitGetTypeTransformerCall([NotNull] ILGenerator il, [NotNull] Type complexType)
        {
            var getTransformerFunc = typeof(TypeTransformerRepository).GetMethod(nameof(TypeTransformerRepository.GetTypeTransformer));
            var repoProperty = typeof(NativeLibraryBase).GetProperty
            (
                nameof(NativeLibraryBase.TransformerRepository),
                BindingFlags.Public | BindingFlags.Instance
            );

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Call, repoProperty.GetMethod); // Get the type transformer repository

            il.EmitTypeOf(complexType);
            il.Emit(OpCodes.Callvirt, getTransformerFunc); // Get the relevant type transformer
        }

        /// <summary>
        /// Lowers the provided type using its corresponding <see cref="ITypeTransformer"/>, if required.
        /// </summary>
        /// <param name="type">The type to lower.</param>
        /// <returns>The type, lowered by its transformer, or the original value.</returns>
        [Pure, NotNull]
        private Type LowerTypeIfRequired([NotNull] Type type)
        {
            if (type.RequiresLowering())
            {
                var transformer = _transformerRepository.GetTypeTransformer(type);
                type = transformer.LowerType();
            }

            return type;
        }
    }
}
