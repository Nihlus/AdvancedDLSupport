//
//  IIntrospectiveMember.cs
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
using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace AdvancedDLSupport.Reflection
{
    /// <summary>
    /// Introspective member interface.
    /// </summary>
    [PublicAPI]
    public interface IIntrospectiveMember
    {
        /// <summary>
        /// Gets the name of the member.
        /// </summary>
        [PublicAPI, NotNull]
        string Name { get; }

        /// <summary>
        /// Gets a custom attribute of <typeparamref name="TAttribute"/>, or null if none can be found.
        /// </summary>
        /// <typeparam name="TAttribute">The type of attribute to get.</typeparam>
        /// <returns>The attribute, or null.</returns>
        [PublicAPI]
        TAttribute? GetCustomAttribute<TAttribute>() where TAttribute : Attribute;

        /// <summary>
        /// Gets the full native entrypoint of the member. This is the configured native entrypoint, with any
        /// transformations applied.
        /// </summary>
        /// <returns>The native entrypoint.</returns>
        [PublicAPI, Pure, NotNull]
        string GetFullNativeEntrypoint();

        /// <summary>
        /// Gets the full unmangled native entrypoint of the member. This is the configured native entrypoint, with any
        /// transformations except name mangling applied.
        /// </summary>
        /// <returns>The native entrypoint.</returns>
        [PublicAPI, Pure, NotNull]
        string GetFullUnmangledNativeEntrypoint();

        /// <summary>
        /// Gets the native entrypoint of the member. This is just the configured native entrypoint, without any
        /// transformations applied.
        /// </summary>
        /// <returns>The native entrypoint.</returns>
        [PublicAPI, Pure, NotNull]
        string GetNativeEntrypoint();

        /// <summary>
        /// Gets the native calling convention of the member.
        /// </summary>
        /// <returns>The calling convention.</returns>
        [PublicAPI, Pure]
        CallingConvention GetNativeCallingConvention();
    }
}
