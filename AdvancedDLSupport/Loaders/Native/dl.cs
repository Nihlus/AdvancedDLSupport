//
//  dl.cs
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

// ReSharper disable InconsistentNaming
#pragma warning disable SA1300 // Elements should begin with an uppercase letter
#pragma warning disable SA1600, CS1591 // Elements should be documented

namespace AdvancedDLSupport.Loaders
{
    /// <summary>
    /// Native libdl methods and constants. Unfortunately, the BSD family of operating systems store their dl functions
    /// in the C standard library, and not in libdl. Therefore, two internal classes have been added as a workaround.
    ///
    /// It should be noted that macOS, while strictly a BSD, hosts a shim libdl library which redirects to libc.
    /// </summary>
    internal static class dl
    {
        private const string LibraryNameUnix = "dl";
        private const string LibraryNameBSD = "c";

        public static IntPtr open(string? fileName, SymbolFlag flags = SymbolFlag.RTLD_DEFAULT, bool useCLibrary = false)
        {
            return useCLibrary ? BSD.dlopen(fileName, flags) : Unix.dlopen(fileName, flags);
        }

        [Pure]
        public static IntPtr sym(IntPtr handle, string name, bool useCLibrary = false)
        {
            return useCLibrary ? BSD.dlsym(handle, name) : Unix.dlsym(handle, name);
        }

        public static int close(IntPtr handle, bool useCLibrary = false)
        {
            return useCLibrary ? BSD.dlclose(handle) : Unix.dlclose(handle);
        }

        public static IntPtr error(bool useCLibrary = false)
        {
            return useCLibrary ? BSD.dlerror() : Unix.dlerror();
        }

        public static void ResetError(bool useCLibrary = false)
        {
            // Clear any outstanding errors by looping until no error is found
            while (error(useCLibrary) != IntPtr.Zero)
            {
            }
        }

        private static class Unix
        {
            [DllImport(LibraryNameUnix)]
            public static extern IntPtr dlopen(string fileName, SymbolFlag flags);

            [DllImport(LibraryNameUnix)]
            public static extern IntPtr dlsym(IntPtr handle, string name);

            [DllImport(LibraryNameUnix)]
            public static extern int dlclose(IntPtr handle);

            [DllImport(LibraryNameUnix)]
            public static extern IntPtr dlerror();
        }

        private static class BSD
        {
            [DllImport(LibraryNameBSD)]
            public static extern IntPtr dlopen(string fileName, SymbolFlag flags);

            [DllImport(LibraryNameBSD)]
            public static extern IntPtr dlsym(IntPtr handle, string name);

            [DllImport(LibraryNameBSD)]
            public static extern int dlclose(IntPtr handle);

            [DllImport(LibraryNameBSD)]
            public static extern IntPtr dlerror();
        }
    }
}
