//
//  BSDPlatformLoader.cs
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
using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace AdvancedDLSupport.Loaders
{
    /// <summary>
    /// Loads libraries on BSD-based platform.
    /// </summary>
    internal sealed class BSDPlatformLoader : PlatformLoaderBase
    {
        /// <summary>
        /// Load the given library with the given flags.
        /// </summary>
        /// <param name="path">The path to the library.</param>
        /// <param name="flags">The loading flags to use.</param>
        /// <returns>A handle to the library. This value carries no intrinsic meaning.</returns>
        /// <exception cref="LibraryLoadingException">Thrown if the library could not be loaded.</exception>
        private IntPtr LoadLibrary([CanBeNull] string path, SymbolFlags flags)
        {
            dl.ResetError(true);

            var libraryHandle = dl.open(path, flags, true);
            if (libraryHandle != IntPtr.Zero)
            {
                return libraryHandle;
            }

            var errorPtr = dl.error(true);
            if (errorPtr == IntPtr.Zero)
            {
                throw new LibraryLoadingException("Library could not be loaded, and error information from dl library could not be found.");
            }

            throw new LibraryLoadingException(string.Format("Library could not be loaded: {0}", Marshal.PtrToStringAnsi(errorPtr)));
        }

        /// <inheritdoc />
        protected override IntPtr LoadLibraryInternal(string path) => LoadLibrary(path, SymbolFlags.RTLD_DEFAULT);

        /// <inheritdoc />
        public override IntPtr LoadSymbol(IntPtr library, string symbolName)
        {
            dl.ResetError(true);

            var symbolHandle = dl.sym(library, symbolName, true);
            if (symbolHandle != IntPtr.Zero)
            {
                return symbolHandle;
            }

            var errorPtr = dl.error(true);
            if (errorPtr == IntPtr.Zero)
            {
                throw new SymbolLoadingException("Symbol could not be loaded, and error information from dl could not be found.");
            }

            var msg = Marshal.PtrToStringAnsi(errorPtr);
            throw new SymbolLoadingException(string.Format("Symbol could not be loaded: {0}", msg));
        }

        /// <inheritdoc />
        public override bool CloseLibrary(IntPtr library)
        {
            return dl.close(library, true) <= 0;
        }
    }
}
