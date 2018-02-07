//
//  IImplementationGenerator.cs
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

using System.Reflection;
using JetBrains.Annotations;

namespace AdvancedDLSupport.ImplementationGenerators
{
    /// <summary>
    /// Interface for classes that generate anonymous implementations for members.
    /// </summary>
    /// <typeparam name="TAccepted">The type of member that the class will generate for.</typeparam>
    internal interface IImplementationGenerator<in TAccepted> where TAccepted : MemberInfo
    {
        /// <summary>
        /// Gets the implementation configuration object to use.
        /// </summary>
        ImplementationOptions Options { get; }

        /// <summary>
        /// Generates a definition and implementation for the given member.
        /// </summary>
        /// <param name="member">The member to generate the implementation for.</param>
        void GenerateImplementation([NotNull] TAccepted member);
    }
}
