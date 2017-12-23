using System;
using JetBrains.Annotations;

namespace AdvancedDLSupport.Loaders
{
    /// <summary>
    /// Represents a class which can load libraries and symbols on a specific platform.
    /// </summary>
    internal interface IPlatformLoader
    {
        /// <summary>
        /// Loads the given symbol name and marshals it into a function delegate.
        /// </summary>
        /// <param name="library">The library handle.</param>
        /// <param name="symbolName">The name of the symbol.</param>
        /// <typeparam name="T">The delegate type to marshal.</typeparam>
        /// <returns>A marshalled delegate.</returns>
        [NotNull, Pure]
        T LoadFunction<T>(IntPtr library, [NotNull] string symbolName);

        /// <summary>
        /// Load the given library.
        /// </summary>
        /// <param name="path">The path to the library.</param>
        /// <returns>A handle to the library. This value carries no intrinsic meaning.</returns>
        /// <exception cref="LibraryLoadingException">Thrown if the library could not be loaded.</exception>
        IntPtr LoadLibrary([NotNull] string path);

        /// <summary>
        /// Load the given symbol.
        /// </summary>
        /// <param name="library">The handle to the library in which the symbol exists.</param>
        /// <param name="symbolName">The name of the symbol to load.</param>
        /// <exception cref="SymbolLoadingException">Thrown if the symbol could not be loaded.</exception>
        /// <returns>A handle to the symbol.</returns>
        [Pure]
        IntPtr LoadSymbol(IntPtr library, [NotNull] string symbolName);

        /// <summary>
        /// Closes the open handle to the given library.
        /// </summary>
        /// <param name="library">The handle to the library to close.</param>
        /// <returns>true if the library was closed successfully; otherwise, false.</returns>
        bool CloseLibrary(IntPtr library);
    }
}
