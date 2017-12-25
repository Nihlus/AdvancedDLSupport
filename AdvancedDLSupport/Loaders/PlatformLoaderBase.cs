using System;
using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace AdvancedDLSupport.Loaders
{
    /// <summary>
    /// Acts as the base for platform loaders.
    /// </summary>
    internal abstract class PlatformLoaderBase : IPlatformLoader
    {
        /// <inheritdoc />
        public T LoadFunction<T>(IntPtr library, string symbolName) =>
            Marshal.GetDelegateForFunctionPointer<T>(LoadSymbol(library, symbolName));

        /// <inheritdoc />
        public IntPtr LoadLibrary(string path)
        {
            // TODO: make local search first configurable
            var resolveResult = DynamicLinkLibraryPathResolver.ResolveAbsolutePath(path, true);
            if (resolveResult.IsSuccess)
            {
                return LoadLibraryInternal(resolveResult.Path);
            }

            var ex = resolveResult.Exception is null
                ? new LibraryLoadingException("Could not find the specified library.")
                : new LibraryLoadingException("Could not find the specified library.", resolveResult.Exception);

            throw ex;
        }

        /// <summary>
        /// Load the given library.
        /// </summary>
        /// <param name="path">The path to the library.</param>
        /// <returns>A handle to the library. This value carries no intrinsic meaning.</returns>
        /// <exception cref="LibraryLoadingException">Thrown if the library could not be loaded.</exception>
        protected abstract IntPtr LoadLibraryInternal([CanBeNull] string path);

        /// <inheritdoc />
        [Pure]
        public abstract IntPtr LoadSymbol(IntPtr library, string symbolName);

        /// <inheritdoc />
        public abstract bool CloseLibrary(IntPtr library);

        /// <summary>
        /// Selects the appropriate platform loader based on the current platform.
        /// </summary>
        /// <returns>A platform loader for the current platform..</returns>
        /// <exception cref="PlatformNotSupportedException">Thrown if the current platform is not supported.</exception>
        [Pure]
        public static IPlatformLoader SelectPlatformLoader()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return new WindowsPlatformLoader();
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return new LinuxPlatformLoader();
            }

            /*
                Temporary hack until BSD is added to RuntimeInformation. OSDescription should contain the output from
                "uname -srv", which will report something along the lines of FreeBSD or OpenBSD plus some more info.
            */
            bool isBSD = RuntimeInformation.OSDescription.ToUpperInvariant().Contains("BSD");
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX) || isBSD)
            {
                return new BSDPlatformLoader();
            }

            throw new PlatformNotSupportedException($"Cannot load native libraries on this platform: {RuntimeInformation.OSDescription}");
        }
    }
}
