//
//  ILibraryPathResolver.cs
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
using System.IO;
using JetBrains.Annotations;

namespace AdvancedDLSupport
{
    /// <summary>
    /// Resolves library paths.
    /// </summary>
    [PublicAPI]
    public interface ILibraryPathResolver
    {
        /// <summary>
        /// Resolves the absolute path to the given library. A null return value signifies the main program.
        /// </summary>
        /// <param name="library">The name or path of the library to load.</param>
        /// <returns>The absolute path to the library.</returns>
        /// <exception cref="PlatformNotSupportedException">
        /// Thrown if the current platform doesn't have a path
        /// resolver defined.</exception>
        /// <exception cref="FileNotFoundException">Thrown if no library file can be found.</exception>
        [PublicAPI, Pure]
        ResolvePathResult Resolve([NotNull] string library);
    }
}
