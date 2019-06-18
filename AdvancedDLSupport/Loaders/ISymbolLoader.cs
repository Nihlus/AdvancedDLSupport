//
//  ISymbolLoader.cs
//
//  Copyright (c) 2018 Firwood Software
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
