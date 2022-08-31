//
//  IImplementationGenerator.cs
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

using System.Collections.Generic;
using System.Reflection;
using AdvancedDLSupport.Pipeline;
using JetBrains.Annotations;

namespace AdvancedDLSupport.ImplementationGenerators
{
    /// <summary>
    /// Interface for classes that generate anonymous implementations for members.
    /// </summary>
    /// <typeparam name="TAccepted">The type of member that the class will generate for.</typeparam>
    [PublicAPI]
    public interface IImplementationGenerator<TAccepted> where TAccepted : MemberInfo
    {
        /// <summary>
        /// Gets the implementation configuration object to use.
        /// </summary>
        [PublicAPI]
        ImplementationOptions Options { get; }

        /// <summary>
        /// Gets the complexity levels of the implementation generator.
        /// </summary>
        [PublicAPI]
        GeneratorComplexity Complexity { get; }

        /// <summary>
        /// Determines whether or not the implementation generator is applicable for the given member definition.
        /// </summary>
        /// <param name="member">The member definition.</param>
        /// <returns>true if the generator is applicable; otherwise, false.</returns>
        [PublicAPI, Pure]
        bool IsApplicable([NotNull] TAccepted member);

        /// <summary>
        /// Generates an implementation for the given member, optionally producing more definitions for processing.
        /// </summary>
        /// <param name="workUnit">The member to generate the implementation for.</param>
        /// <returns>An optional set of more definitions to be processed.</returns>
        [PublicAPI, NotNull, ItemNotNull]
        IEnumerable<PipelineWorkUnit<TAccepted>> GenerateImplementation([NotNull] PipelineWorkUnit<TAccepted> workUnit);
    }
}
