//
//  IEntrypointMangler.cs
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

using System.Reflection;
using AdvancedDLSupport.Reflection;
using JetBrains.Annotations;

namespace AdvancedDLSupport
{
    /// <summary>
    /// Represents a class that can mangle entrypoint names according to an implementation-specific pattern.
    /// </summary>
    [PublicAPI, UsedImplicitly]
    public interface IEntrypointMangler
    {
        /// <summary>
        /// Determines whether or not the mangler is applicable to the given member.
        /// </summary>
        /// <param name="member">The member to check.</param>
        /// <returns>true if the mangler is applicable; otherwise, false.</returns>
        [PublicAPI]
        bool IsManglerApplicable([NotNull] MemberInfo member);

        /// <summary>
        /// Mangles the given member.
        /// </summary>
        /// <typeparam name="T">The type of the member to mangle.</typeparam>
        /// <param name="member">The member to mangle.</param>
        /// <returns>The mangled entrypoint.</returns>
        [PublicAPI, NotNull]
        string Mangle<T>([NotNull] T member) where T : IIntrospectiveMember;

        /// <summary>
        /// Demangles a mangled entrypoint name, returning it to its original state.
        /// </summary>
        /// <param name="mangledEntrypoint">A mangled entrypoint name.</param>
        /// <returns>The demangled entrypoint.</returns>
        [PublicAPI, NotNull]
        string Demangle([NotNull] string mangledEntrypoint);
    }
}
