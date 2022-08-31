//
//  ImplementationGeneratorSorter.cs
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
using Mono.DllMap.Extensions;
using static AdvancedDLSupport.ImplementationGenerators.GeneratorComplexity;

namespace AdvancedDLSupport.ImplementationGenerators;

/// <summary>
/// Sorts a set of input generators based on their complexity.
/// </summary>
internal class ImplementationGeneratorSorter
{
    /// <summary>
    /// Sorts the input generators based on their complexity.
    /// </summary>
    /// <param name="generators">The input generators.</param>
    /// <typeparam name="T">The member that the generators accept.</typeparam>
    /// <returns>The sorted generators.</returns>
    public IEnumerable<IImplementationGenerator<T>> SortGenerators<T>
    (
        IEnumerable<IImplementationGenerator<T>> generators
    )
        where T : MemberInfo
    {
        var generatorGroups = generators
            .OrderByDescending(c => CalculateComplexityScore(c.Complexity))
            .GroupBy
            (
                c =>
                    c.Complexity.HasFlagFast(Terminating)
            ).OrderBy(c => c.Key);

        return generatorGroups.SelectMany(g => g);
    }

    /// <summary>
    /// Calculates a generator's complexity score. A higher score means a more complex generator.
    /// </summary>
    /// <param name="complexity">The complexity flags.</param>
    /// <returns>An integer value that represents the complexity of the flags.</returns>
    private int CalculateComplexityScore(GeneratorComplexity complexity)
    {
        var score = 0;

        foreach (var value in Enum.GetValues(typeof(GeneratorComplexity)).Cast<GeneratorComplexity>().Except(new[] { Terminating }))
        {
            if (complexity.HasFlagFast(value))
            {
                ++score;
            }
        }

        return score;
    }
}
