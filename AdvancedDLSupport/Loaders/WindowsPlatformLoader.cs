//
//  WindowsPlatformLoader.cs
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
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace AdvancedDLSupport.Loaders
{
    /// <summary>
    /// Loads libraries on the Windows platform.
    /// </summary>
    internal sealed class WindowsPlatformLoader : PlatformLoaderBase
    {
        /// <inheritdoc />
        protected override IntPtr LoadLibraryInternal(string path)
        {
            if (path is null)
            {
                throw new ArgumentNullException(nameof(path), "null library names or paths are not supported on Windows.");
            }

            var libraryHandle = kernel32.LoadLibrary(path);
            if (libraryHandle == IntPtr.Zero)
            {
                throw new LibraryLoadingException("Library loading failed.", new Win32Exception(Marshal.GetLastWin32Error()), path);
            }

            return libraryHandle;
        }

        /// <inheritdoc />
        public override IntPtr LoadSymbol(IntPtr library, string symbolName)
        {
            var symbolHandle = kernel32.GetProcAddress(library, symbolName);
            if (symbolHandle == IntPtr.Zero)
            {
                throw new SymbolLoadingException("Symbol loading failed.", new Win32Exception(Marshal.GetLastWin32Error()), symbolName);
            }

            return symbolHandle;
        }

        /// <inheritdoc />
        public override bool CloseLibrary(IntPtr library)
        {
            return kernel32.FreeLibrary(library) > 0;
        }
    }
}
