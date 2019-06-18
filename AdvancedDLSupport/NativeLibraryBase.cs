//
//  NativeLibraryBase.cs
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
using AdvancedDLSupport.Loaders;
using JetBrains.Annotations;
using Mono.DllMap.Extensions;
using static AdvancedDLSupport.ImplementationOptions;

namespace AdvancedDLSupport
{
    /// <summary>
    /// Internal base class for library implementations.
    /// </summary>
    [PublicAPI]
    public abstract class NativeLibraryBase : IDisposable
    {
        /// <summary>
        /// Gets a value indicating whether or not the library has been disposed.
        /// </summary>
        [PublicAPI]
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// Gets or sets the set of options that were used to construct the type.
        /// </summary>
        internal ImplementationOptions Options { get; set; }

        /// <summary>
        /// Gets an opaque native handle to the library.
        /// </summary>
        private IntPtr _libraryHandle;

        /// <summary>
        /// Gets the library and symbol loader for the current platform.
        /// </summary>
        [NotNull]
        private static IPlatformLoader PlatformLoader => PlatformLoaderBase.PlatformLoader;

        private readonly ILibraryLoader _libLoader;
        private readonly ISymbolLoader _symbolLoader;

        private ILibraryLoader LibraryLoader => _libLoader ?? PlatformLoader;

        private ISymbolLoader SymbolLoader => _symbolLoader ?? PlatformLoader;

        /// <summary>
        /// Initializes a new instance of the <see cref="NativeLibraryBase"/> class.
        /// </summary>
        /// <param name="path">The path to the library.</param>
        /// <param name="options">Whether or not this library can be disposed.</param>
        [PublicAPI, AnonymousConstructor]
        protected NativeLibraryBase
        (
            [CanBeNull] string path,
            ImplementationOptions options
        )
        {
            Options = options;
            _libraryHandle = PlatformLoader.LoadLibrary(path);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NativeLibraryBase"/> class.
        /// </summary>
        /// <param name="path">The path to the library.</param>
        /// <param name="options">Whether or not this library can be disposed.</param>
        /// <param name="libLoader">Overriding library loader.</param>
        /// <param name="symLoader">Overriding symbol loader.</param>
        [PublicAPI]
        protected NativeLibraryBase
        (
            [CanBeNull] string path,
            ImplementationOptions options,
            [CanBeNull] ILibraryLoader libLoader,
            [CanBeNull] ISymbolLoader symLoader
        )
        {
            _libLoader = libLoader;
            _symbolLoader = symLoader;
            Options = options;
            _libraryHandle = LibraryLoader.LoadLibrary(path);
        }

        /// <summary>
        /// Forwards the symbol loading call to the wrapped platform loader.
        /// </summary>
        /// <param name="sym">The symbol name.</param>
        /// <returns>A handle to the symbol.</returns>
        internal IntPtr LoadSymbol([NotNull] string sym) => SymbolLoader.LoadSymbol(_libraryHandle, sym);

        /// <summary>
        /// Forwards the function loading call to the wrapped platform loader.
        /// </summary>
        /// <param name="sym">The symbol name.</param>
        /// <typeparam name="T">The delegate to load the symbol as.</typeparam>
        /// <returns>A function delegate.</returns>
        [NotNull]
        internal T LoadFunction<T>([NotNull] string sym) => SymbolLoader.LoadFunction<T>(_libraryHandle, sym);

        /// <summary>
        /// Throws if the library has been disposed.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Thrown if the library has been disposed.</exception>
        [PublicAPI]
        protected void ThrowIfDisposed()
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(GetType().Name, "The library has been disposed.");
            }
        }

        /// <inheritdoc />
        [PublicAPI]
        public void Dispose()
        {
            if (IsDisposed || !Options.HasFlagFast(GenerateDisposalChecks))
            {
                return;
            }

            IsDisposed = true;

            LibraryLoader.CloseLibrary(_libraryHandle);
            _libraryHandle = IntPtr.Zero;
        }
    }
}
