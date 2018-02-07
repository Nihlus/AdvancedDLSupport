//
//  PermutationGenerator.cs
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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AdvancedDLSupport.Extensions;
using AdvancedDLSupport.Reflection;

namespace AdvancedDLSupport
{
    /// <summary>
    /// Helper class for generating parameter permutations for methods with <see cref="Nullable{T}"/> parameters that
    /// are passed by reference.
    /// </summary>
    public class PermutationGenerator
    {
        /// <summary>
        /// Generates all possible permutations of either a raw struct passed by reference, or an IntPtr, given a
        /// method containing <see cref="Nullable{T}"/>s, passed by reference.
        /// </summary>
        /// <param name="baseMethod">The method to generate permutatations of.</param>
        /// <returns>The permutations.</returns>
        public IReadOnlyList<IReadOnlyList<Type>> Generate(IntrospectiveMethodInfo baseMethod)
        {
            var parameters = baseMethod.ParameterTypes;

            // First, we calculate the total number of possible combinations, given that we can have either a
            // concrete type or an IntPtr, and refNullableParameterCount instances thereof.
            var refNullableParameterCount = parameters.Count(p => p.IsRefNullable());
            var permutationCount = Math.Pow
            (
                2,
                refNullableParameterCount
            );

            // Then, we take the types used in the base method and generate combinations from it.
            var permutations = new List<IReadOnlyList<Type>>();
            for (int i = 0; i < permutationCount; ++i)
            {
                // Due to the fact that we only need to flip between two states for each instance of a nullable
                // parameter, we can piggyback on the permutation count and use it as a bitmask which determines
                // what to flip each parameter to.
                var bits = new BitArray(new[] { i });
                permutations.Add(GeneratePermutation(parameters, bits));
            }

            return permutations;
        }

        /// <summary>
        /// Generates a permutation of the given original parameter types, using the given <see cref="BitArray"/> to
        /// mutate the parameters that is a <see cref="Nullable{T}"/> passed by reference.
        /// </summary>
        /// <param name="basePermutation">The base set of parameter types.</param>
        /// <param name="mask">The bit mask to use for mutation.</param>
        /// <returns>The permutation.</returns>
        private IReadOnlyList<Type> GeneratePermutation(IReadOnlyList<Type> basePermutation, BitArray mask)
        {
            // For each type in the base permutation (containing nullable refs), we inspect the type
            var skipped = 0;
            var newPermutation = new Type[basePermutation.Count];
            for (int i = 0; i < basePermutation.Count; ++i)
            {
                var type = basePermutation[i];
                if (!type.IsRefNullable())
                {
                    // If it's not a nullable passed by reference, we'll just pass it through as normal.
                    // Furthermore, in order to maintain alignment with the bitmask, we add to the negative offset
                    // of "skipped" types.
                    newPermutation[i] = type;
                    ++skipped;
                    continue;
                }

                // Then, we pick out the mask value, offset by the number of irrelevant types we've skipped
                var maskValue = mask[i - skipped];

                var newPermutationType = maskValue
                    ? type.GetElementType().GetGenericArguments().First().MakeByRefType()
                    : typeof(IntPtr);

                // And assign the result to the correct position
                newPermutation[i] = newPermutationType;
            }

            return newPermutation;
        }
    }
}
