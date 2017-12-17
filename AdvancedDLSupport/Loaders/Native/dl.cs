using System;
using System.Runtime.InteropServices;

using static AdvancedDLSupport.SymbolFlags;

// ReSharper disable InconsistentNaming
#pragma warning disable SA1300 // Elements should begin with an uppercase letter
#pragma warning disable SA1600, CS1591 // Elements should be documented

namespace AdvancedDLSupport
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

        public static IntPtr open(string fileName, SymbolFlags flags = RTLD_DEFAULT, bool useCLibrary = false)
        {
            return useCLibrary ? BSD.dlopen(fileName, flags) : Unix.dlopen(fileName, flags);
        }

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

        private static class Unix
        {
            [DllImport(LibraryNameUnix)]
            public static extern IntPtr dlopen(string fileName, SymbolFlags flags);

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
            public static extern IntPtr dlopen(string fileName, SymbolFlags flags);

            [DllImport(LibraryNameBSD)]
            public static extern IntPtr dlsym(IntPtr handle, string name);

            [DllImport(LibraryNameBSD)]
            public static extern int dlclose(IntPtr handle);

            [DllImport(LibraryNameBSD)]
            public static extern IntPtr dlerror();
        }
    }
}
