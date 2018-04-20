//
//  RefPermutationImplementationGenerator.cs
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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using AdvancedDLSupport.Extensions;
using AdvancedDLSupport.Pipeline;
using AdvancedDLSupport.Reflection;
using JetBrains.Annotations;
using static System.Reflection.MethodAttributes;

namespace AdvancedDLSupport.ImplementationGenerators
{
    /// <summary>
    /// Generates a set of method permutations, based on a method with <see cref="Nullable{T}"/> parameters passed by
    /// reference.
    /// </summary>
    internal sealed class RefPermutationImplementationGenerator : ImplementationGeneratorBase<IntrospectiveMethodInfo>
    {
        private readonly PermutationGenerator _permutationGenerator;

        [NotNull]
        private readonly TypeTransformerRepository _transformerRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="RefPermutationImplementationGenerator"/> class.
        /// </summary>
        /// <param name="targetModule">The module in which the method implementation should be generated.</param>
        /// <param name="targetType">The type in which the method implementation should be generated.</param>
        /// <param name="targetTypeConstructorIL">The IL generator for the target type's constructor.</param>
        /// <param name="options">The configuration object to use.</param>
        /// <param name="transformerRepository">The repository where type transformers are stored.</param>
        public RefPermutationImplementationGenerator
        (
            [NotNull] ModuleBuilder targetModule,
            [NotNull] TypeBuilder targetType,
            [NotNull] ILGenerator targetTypeConstructorIL,
            ImplementationOptions options,
            [NotNull] TypeTransformerRepository transformerRepository
        )
            : base(targetModule, targetType, targetTypeConstructorIL, options)
        {
            _permutationGenerator = new PermutationGenerator();

            _transformerRepository = transformerRepository;
        }

        /// <inheritdoc />
        public override IEnumerable<PipelineWorkUnit<IntrospectiveMethodInfo>> GenerateImplementation(PipelineWorkUnit<IntrospectiveMethodInfo> workUnit)
        {
            var permutations = new List<IntrospectiveMethodInfo>();
            var definition = workUnit.Definition;

            // Generate permutation definitions
            var parameterPermutations = _permutationGenerator.Generate(definition);
            for (int i = 0; i < parameterPermutations.Count; ++i)
            {
                var permutation = parameterPermutations[i];
                var method = TargetType.DefineMethod
                (
                    $"{workUnit.GetUniqueBaseMemberName()}_permutation_{i}",
                    Public | Final | Virtual | HideBySig | NewSlot,
                    CallingConventions.Standard,
                    definition.ReturnType,
                    permutation.ToArray()
                );

                var methodInfo = new IntrospectiveMethodInfo(method, definition.ReturnType, permutation, definition);
                permutations.Add(methodInfo);
            }

            GenerateTopLevelMethodImplementation(definition, permutations);

            // Pass the permutations on for further processing
            foreach (var permutation in permutations)
            {
                yield return new PipelineWorkUnit<IntrospectiveMethodInfo>(permutation, workUnit);
            }
        }

        /// <summary>
        /// Generates the implementation body of the top-level method, producing a method that selects the appropriate
        /// permutation to use for its runtime input.
        /// </summary>
        /// <param name="definition">The <see cref="MethodInfo"/> describing the base definition of the method.</param>
        /// <param name="permutations">The generated methods.</param>
        private void GenerateTopLevelMethodImplementation
        (
            [NotNull] IntrospectiveMethodInfo definition,
            [NotNull, ItemNotNull] IReadOnlyList<IntrospectiveMethodInfo> permutations
        )
        {
            if (!(definition.GetWrappedMember() is MethodBuilder builder))
            {
                throw new ArgumentNullException(nameof(definition), "Could not unwrap introspective method to method builder.");
            }

            var nextFreeLocalSlot = 0;

            var parameterTypes = definition.ParameterTypes.ToList();
            var methodIL = builder.GetILGenerator();

            // Create a boolean array to store the data pertaining to which parameters have values
            var hasValueArrayIndex = EmitHasValueArray(methodIL, ref nextFreeLocalSlot, parameterTypes);

            methodIL.Emit(OpCodes.Ldloc, hasValueArrayIndex);
            EmitPermutationIndex(methodIL);

            // Generate a jump table for the upcoming switch
            var labelList = new List<Label>();
            for (int i = 0; i < permutations.Count; ++i)
            {
                labelList.Add(methodIL.DefineLabel());
            }

            var defaultCase = methodIL.DefineLabel();
            var endOfSwitch = methodIL.DefineLabel();

            var jumpTable = labelList.ToArray();

            methodIL.Emit(OpCodes.Switch, jumpTable);
            methodIL.Emit(OpCodes.Br, defaultCase);

            EmitDefaultSwitchCase(methodIL, defaultCase);

            // Emit the switch cases, one for each permutation
            for (int i = 0; i < permutations.Count; ++i)
            {
                methodIL.MarkLabel(jumpTable[i]);

                var permutationTypes = permutations[i].ParameterTypes;

                methodIL.Emit(OpCodes.Ldarg_0);
                for (int argumentIndex = 1; argumentIndex < permutationTypes.Count + 1; ++argumentIndex)
                {
                    var originalParameterType = parameterTypes[argumentIndex - 1];
                    var permutationParameterType = permutationTypes[argumentIndex - 1];
                    if (originalParameterType.IsRefNullable() && permutationParameterType != typeof(IntPtr))
                    {
                        // ReSharper disable once PossibleNullReferenceException
                        var wrappedType = originalParameterType.GetElementType().GetGenericArguments().First();
                        EmitNullableValueRef(methodIL, argumentIndex, ref nextFreeLocalSlot, wrappedType);
                    }
                    else if (permutationParameterType == typeof(IntPtr) && parameterTypes[argumentIndex - 1].IsRefNullable())
                    {
                        // We know that this is null
                        var zeroPointerField = typeof(IntPtr).GetField
                        (
                            nameof(IntPtr.Zero),
                            BindingFlags.Public | BindingFlags.Static
                        );

                        methodIL.Emit(OpCodes.Ldsfld, zeroPointerField);
                    }
                    else
                    {
                        methodIL.Emit(OpCodes.Ldarg, argumentIndex);
                    }
                }

                // Call the permutation
                methodIL.Emit(OpCodes.Call, permutations[i].GetWrappedMember());

                // break;
                methodIL.Emit(OpCodes.Br, endOfSwitch);
            }

            // Mark end of switch
            methodIL.MarkLabel(endOfSwitch);
            methodIL.Emit(OpCodes.Ret);
        }

        /// <summary>
        /// Emits a set of IL instructions that take the argument at the given index, treating it as a
        /// <see cref="Nullable{T}"/>, converting it into a ByRef T, pointing to the internal value of the nullable, and
        /// pushes it onto the evaluation stack.
        /// </summary>
        /// <param name="il">The IL generator where the instructions should be emitted.</param>
        /// <param name="argumentIndex">The argument index to load.</param>
        /// <param name="nextFreeLocalSlot">The next unused local variable index.</param>
        /// <param name="wrappedType">The type wrapped by the nullable.</param>
        private static void EmitNullableValueRef
        (
            [NotNull] ILGenerator il,
            int argumentIndex,
            ref int nextFreeLocalSlot,
            [NotNull] Type wrappedType
        )
        {
            // Declare a pinned pointer local
            var localIndex = nextFreeLocalSlot;
            ++nextFreeLocalSlot;

            il.DeclareLocal(typeof(int*), true);

            // Now, we load the nullable as a pointer
            EmitGetPinnedAddressOfNullable(il, typeof(Nullable<>).MakeGenericType(wrappedType), argumentIndex);

            // Store the value so that it gets pinned
            il.Emit(OpCodes.Stloc, localIndex);

            // And extract a reference to the internal value
            il.Emit(OpCodes.Ldloc, localIndex);
            EmitAccessInternalNullableValue(il, wrappedType);
        }

        /// <summary>
        /// Emits a set of IL instructions that loads the argument at the given index,
        /// treating it as a <see cref="Nullable{T}"/>, and gets a pinned pointer to it, pushing it onto the evaluation stack.
        /// </summary>
        /// <param name="il">The IL generator where the instructions should be emitted.</param>
        /// <param name="nullableType">The type that the nullable wraps.</param>
        /// <param name="argumentIndex">The argument index to load.</param>
        private static void EmitGetPinnedAddressOfNullable
        (
            [NotNull] ILGenerator il,
            [NotNull] Type nullableType,
            int argumentIndex
        )
        {
            il.Emit(OpCodes.Ldarg, argumentIndex);

            var unsafeAsMethod = typeof(Unsafe).GetMethods().First
            (
                m =>
                    m.Name == nameof(Unsafe.As) &&
                    m.GetParameters().First().ParameterType.IsByRef
            )
            .MakeGenericMethod(nullableType, typeof(int));

            il.Emit(OpCodes.Call, unsafeAsMethod);
        }

        /// <summary>
        /// Emits a set of IL instructs that takes a value on the evaluation stack, treating it as a pointer to an
        /// instance of a <see cref="Nullable{T}"/>, and retrieves a ByRef handle to its internal wrapped struct,
        /// pushing it onto the evaluation stack.
        /// </summary>
        /// <param name="il">The IL generator where the instructions should be emitted.</param>
        /// <param name="wrappedType">The type that the nullable wraps.</param>
        private static void EmitAccessInternalNullableValue([NotNull] ILGenerator il, [NotNull] Type wrappedType)
        {
            // ReSharper disable once PossibleNullReferenceException
            var accessValueMethod = typeof(InternalNullableAccessor)
                .GetMethod(nameof(InternalNullableAccessor.AccessUnderlyingValue))
                .MakeGenericMethod(wrappedType);

            il.Emit(OpCodes.Conv_I);
            il.Emit(OpCodes.Call, accessValueMethod);
        }

        /// <summary>
        /// Emits a set of IL instructions that creates a new <see cref="BitArray"/>, constructing it from a
        /// <see cref="bool"/> array on the evaluation stack, and then converts it to an <see cref="int"/>, placing it
        /// back on the evaluation stack.
        /// </summary>
        /// <param name="il">The IL generator where the instructions should be emitted.</param>
        private static void EmitPermutationIndex([NotNull] ILGenerator il)
        {
            // Create a new BitArray to use as the mask
            var bitArrayConstructor = typeof(BitArray).GetConstructor(new[] { typeof(bool[]) });
            il.Emit(OpCodes.Newobj, bitArrayConstructor);

            var bitArrayToInteger = typeof(BitArrayExtensions).GetMethod(nameof(BitArrayExtensions.ToInt32));
            il.Emit(OpCodes.Call, bitArrayToInteger);
        }

        /// <summary>
        /// Emits a set of IL instructions that creates a new <see cref="bool"/> array, storing it in the local variable
        /// with the given index. Then, it inspects all parameters for instances of <see cref="Nullable{T}"/> parameters, and
        /// stores whether or not that parameter has an underlying value in the array.
        /// </summary>
        /// <param name="il">The IL generator where the instructions should be emitted.</param>
        /// <param name="nextFreeLocalSlot">The next unused local variable index.</param>
        /// <param name="parameters">The method parameters.</param>
        /// <returns>The index of the local variable used for the array.</returns>
        private static int EmitHasValueArray
        (
            [NotNull] ILGenerator il,
            ref int nextFreeLocalSlot,
            [NotNull, ItemNotNull] IList<Type> parameters
        )
        {
            var localIndex = nextFreeLocalSlot;
            ++nextFreeLocalSlot;
            il.DeclareLocal(typeof(bool[]));

            var refNullableParameters = parameters.Where
            (
                p =>
                    p.IsRefNullable()
            ).Select
            (
                p =>
                    (Index: parameters.IndexOf(p) + 1, Value: p)
            ).ToList();

            var refNullableCount = refNullableParameters.Count;

            il.Emit(OpCodes.Ldc_I4, refNullableCount);
            il.Emit(OpCodes.Newarr, typeof(bool));
            il.Emit(OpCodes.Stloc, localIndex);

            // Emit checks for whether or not the nullable has a value
            for (int i = 0; i < refNullableCount; ++i)
            {
                il.Emit(OpCodes.Ldloc, localIndex);
                il.Emit(OpCodes.Ldc_I4, i);
                EmitNullableGetHasValue(il, refNullableParameters[i]);

                il.Emit(OpCodes.Stelem_I1);
            }

            return localIndex;
        }

        private static void EmitDefaultSwitchCase([NotNull] ILGenerator methodIL, Label defaultCase)
        {
            // Generate default case
            methodIL.MarkLabel(defaultCase);
            methodIL.Emit(OpCodes.Ldstr, "index");
            methodIL.Emit(OpCodes.Ldstr, "No method permutation known for that index.");

            var exceptionConstructor = typeof(ArgumentOutOfRangeException).GetConstructor
            (
                new[] { typeof(string), typeof(string) }
            );

            methodIL.Emit(OpCodes.Newobj, exceptionConstructor);
            methodIL.Emit(OpCodes.Throw);
        }

        /// <summary>
        /// Emits the IL required for accessing the parameter at a given index, and checking whether or not it has a
        /// value, placing a boolean value onto the evaluation stack.
        /// </summary>
        /// <param name="methodIL">The IL generator where the instructions should be emitted.</param>
        /// <param name="parameter">A <see cref="ValueTuple"/> containing the parameter index, and its associated <see cref="ParameterInfo"/>.</param>
        [SuppressMessage("ReSharper", "PossibleNullReferenceException", Justification = "Known at compile time.")]
        private static void EmitNullableGetHasValue([NotNull] ILGenerator methodIL, (int Index, Type Type) parameter)
        {
            methodIL.Emit(OpCodes.Ldarg, parameter.Index);

            var getHasValueMethod = parameter.Type
                .GetElementType()
                .GetProperty(nameof(Nullable<int>.HasValue))
                .GetGetMethod();

            methodIL.Emit(OpCodes.Call, getHasValueMethod);
        }
    }
}
