//
//  GeneratorComplexity.cs
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
using JetBrains.Annotations;

namespace AdvancedDLSupport.ImplementationGenerators
{
    /// <summary>
    /// Represents levels of complexity in a generator.
    /// </summary>
    [PublicAPI, Flags]
    public enum GeneratorComplexity
    {
        /// <summary>
        /// The generator does not perform any operations that are considered complex.
        /// </summary>
        None = 0,

        /// <summary>
        /// The generator transforms the parameters of the input definition in some way.
        /// </summary>
        TransformsParameters = 1 << 0,

        /// <summary>
        /// The generator alters its behaviour based on data about the input member - skipping it altogether, performing
        /// a different operation, etc.
        /// </summary>
        MemberDependent = 1 << 1,

        /// <summary>
        /// The generator alters its behaviour based on the current active <see cref="ImplementationOptions"/> -
        /// skipping it altogether, performing a different operation, etc.
        /// </summary>
        OptionDependent = 1 << 2,

        /// <summary>
        /// The generator is a terminating generator, and will not produce any output definitions. These generators are
        /// sorted apart from the normal generators, and are always executed last.
        /// </summary>
        Terminating = 1 << 3,

        /// <summary>
        /// The generator will create additional types in the assembly.
        /// </summary>
        CreatesTypes = 1 << 4,

        /// <summary>
        /// The generator defers its real implementation to a later time (usually call-time).
        /// </summary>
        DeferredImplementation = 1 << 5
    }
}
