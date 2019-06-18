using System;
using JetBrains.Annotations;

namespace AdvancedDLSupport.Loaders
{
    /// <summary>
    /// Represents a class which can load Symbols on a specific platform.
    /// </summary>
    public interface ISymbolLoader
    {
        /// <summary>
        /// Load the given symbol.
        /// </summary>
        /// <param name="library">The handle to the library in which the symbol exists.</param>
        /// <param name="symbolName">The name of the symbol to load.</param>
        /// <exception cref="SymbolLoadingException">Thrown if the symbol could not be loaded.</exception>
        /// <returns>A handle to the symbol.</returns>
        [PublicAPI, Pure]
        IntPtr LoadSymbol(IntPtr library, [NotNull] string symbolName);

        /// <summary>
        /// Loads the given symbol name and marshals it into a function delegate.
        /// </summary>
        /// <param name="library">The library handle.</param>
        /// <param name="symbolName">The name of the symbol.</param>
        /// <typeparam name="T">The delegate type to marshal.</typeparam>
        /// <returns>A marshalled delegate.</returns>
        [PublicAPI, NotNull, Pure]
        T LoadFunction<T>(IntPtr library, [NotNull] string symbolName);
    }
}
