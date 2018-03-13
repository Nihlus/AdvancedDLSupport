//
//  IEntrypointMangler.cs
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
using AdvancedDLSupport.Reflection;

namespace AdvancedDLSupport
{
    /// <summary>
    /// Represents a class that can mangle entrypoint names according to an implementation-specific pattern.
    /// </summary>
    public interface IEntrypointMangler
    {
        /// <summary>
        /// Determines whether or not the mangler is applicable to the given
        /// </summary>
        /// <param name="member">The member to check.</param>
        /// <returns>true if the mangler is applicable; otherwise, false.</returns>
        bool IsManglerApplicable(MemberInfo member);

        /// <summary>
        /// Mangles the given member.
        /// </summary>
        /// <typeparam name="T">The type of the member to mangle.</typeparam>
        /// <param name="member">The member to mangle.</param>
        /// <returns>The mangled entrypoint.</returns>
        string Mangle<T>(T member) where T : IIntrospectiveMember;

        /// <summary>
        /// Demangles a mangled entrypoint name, returning it to its original state.
        /// </summary>
        /// <param name="mangledEntrypoint">A mangled entrypoint name.</param>
        /// <returns>The demangled entrypoint.</returns>
        string Demangle(string mangledEntrypoint);
    }
}
