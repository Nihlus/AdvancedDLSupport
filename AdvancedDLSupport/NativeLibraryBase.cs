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
    /// Internal base class for library implementations
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
        /// Gets the type transformer repository.
        /// </summary>
        [NotNull]
        internal TypeTransformerRepository TransformerRepository { get; }

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
        private static readonly IPlatformLoader PlatformLoader;

        /// <summary>
        /// Initializes static members of the <see cref="NativeLibraryBase"/> class.
        /// </summary>
        static NativeLibraryBase()
        {
            PlatformLoader = PlatformLoaderBase.SelectPlatformLoader();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NativeLibraryBase"/> class.
        /// </summary>
        /// <param name="path">The path to the library.</param>
        /// <param name="options">Whether or not this library can be disposed.</param>
        /// <param name="transformerRepository">The repository containing type transformers.</param>
        [PublicAPI, AnonymousConstructor]
        protected NativeLibraryBase
        (
            [NotNull] string path,
            ImplementationOptions options,
            [NotNull] TypeTransformerRepository transformerRepository)
        {
            Options = options;
            TransformerRepository = transformerRepository;
            _libraryHandle = PlatformLoader.LoadLibrary(path);
        }

        /// <summary>
        /// Forwards the symbol loading call to the wrapped platform loader.
        /// </summary>
        /// <param name="sym">The symbol name.</param>
        /// <returns>A handle to the symbol</returns>
        internal IntPtr LoadSymbol([NotNull] string sym) => PlatformLoader.LoadSymbol(_libraryHandle, sym);

        /// <summary>
        /// Forwards the function loading call to the wrapped platform loader.
        /// </summary>
        /// <param name="sym">The symbol name.</param>
        /// <typeparam name="T">The delegate to load the symbol as.</typeparam>
        /// <returns>A function delegate.</returns>
        [NotNull]
        internal T LoadFunction<T>([NotNull] string sym) => PlatformLoader.LoadFunction<T>(_libraryHandle, sym);

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

            PlatformLoader.CloseLibrary(_libraryHandle);
            _libraryHandle = IntPtr.Zero;
        }
    }
}
