//
//  ILibraryLoader.cs
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
using JetBrains.Annotations;

namespace AdvancedDLSupport.Loaders
{
    /// <summary>
    /// Represents a class which can load libraries on a specific platform.
    /// </summary>
    public interface ILibraryLoader
    {
        /// <summary>
        /// Load the given library. A null path signifies intent to load the main executable instead of an external
        /// library.
        /// </summary>
        /// <param name="path">The path to the library.</param>
        /// <returns>A handle to the library. This value carries no intrinsic meaning.</returns>
        /// <exception cref="LibraryLoadingException">Thrown if the library could not be loaded.</exception>
        [PublicAPI]
        IntPtr LoadLibrary([CanBeNull] string path);

        /// <summary>
        /// Closes the open handle to the given library.
        /// </summary>
        /// <param name="library">The handle to the library to close.</param>
        /// <returns>true if the library was closed successfully; otherwise, false.</returns>
        [PublicAPI]
        bool CloseLibrary(IntPtr library);
    }
}
