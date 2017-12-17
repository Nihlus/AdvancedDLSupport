using System;
using System.Runtime.InteropServices;
using static AdvancedDLSupport.SymbolFlags;

namespace AdvancedDLSupport
{
    /// <summary>
    /// Loads libraries on the Linux platform.
    /// </summary>
    internal class LinuxPlatformLoader : PlatformLoaderBase
    {
        /// <summary>
        /// Load the given library with the given flags.
        /// </summary>
        /// <param name="path">The path to the library.</param>
        /// <param name="flags">The loading flags to use.</param>
        /// <returns>A handle to the library. This value carries no intrinsic meaning.</returns>
        /// <exception cref="LibraryLoadingException">Thrown if the library could not be loaded.</exception>
        public IntPtr LoadLibrary(string path, SymbolFlags flags)
        {
            var libraryHandle = dl.open(path, flags);
            if (libraryHandle != IntPtr.Zero)
            {
                return libraryHandle;
            }

            var errorPtr = dl.error();
            if (errorPtr == IntPtr.Zero)
            {
                throw new LibraryLoadingException("Library could not be loaded, and error information from dl library could not be found.");
            }

            var msg = Marshal.PtrToStringAuto(errorPtr);
            throw new LibraryLoadingException(string.Format("Library could not be loaded: {0}", msg));
        }

        /// <inheritdoc />
        public override IntPtr LoadLibrary(string path) => LoadLibrary(path, RTLD_DEFAULT);

        /// <inheritdoc />
        public override IntPtr LoadSymbol(IntPtr library, string symbolName)
        {
            var symbolHandle = dl.sym(library, symbolName);
            if (symbolHandle != IntPtr.Zero)
            {
                return symbolHandle;
            }

            var errorPtr = dl.error();
            if (errorPtr == IntPtr.Zero)
            {
                throw new SymbolLoadingException("Symbol could not be loaded, and error information from dl could not be found.");
            }

            var msg = Marshal.PtrToStringAuto(errorPtr);
            throw new SymbolLoadingException(string.Format("Symbol could not be loaded: {0}", msg));
        }

        /// <inheritdoc />
        public override bool CloseLibrary(IntPtr library)
        {
            return dl.close(library) <= 0;
        }
    }
}
