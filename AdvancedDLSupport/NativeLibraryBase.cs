//
//  NativeLibraryBase.cs
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
        private static readonly IPlatformLoader PlatformLoader;

        static NativeLibraryBase()
        {
            PlatformLoader = PlatformLoaderBase.SelectPlatformLoader();
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="NativeLibraryBase"/> class.
        /// </summary>
        ~NativeLibraryBase()
        {
            Dispose();
        }

        [NotNull]
        private readonly string _path;

        [NotNull]
        private readonly Type _interfaceType;
        private IntPtr _libraryHandle;

        /// <summary>
        /// Gets the type transformer repository.
        /// </summary>
        [PublicAPI]
        public TypeTransformerRepository TransformerRepository { get; }

        /// <summary>
        /// Gets a value indicating whether or not the library has been disposed.
        /// </summary>
        [PublicAPI]
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether or not the library can be disposed.
        /// </summary>
        private ImplementationOptions Options { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="NativeLibraryBase"/> class.
        /// </summary>
        /// <param name="path">The path to the library.</param>
        /// <param name="interfaceType">The interface type that the anonymous type implements.</param>
        /// <param name="options">Whether or not this library can be disposed.</param>
        /// <param name="transformerRepository">The repository containing type transformers.</param>
        [PublicAPI, AnonymousConstructor]
        protected NativeLibraryBase
        (
            [NotNull] string path,
            [NotNull] Type interfaceType,
            ImplementationOptions options,
            [NotNull] TypeTransformerRepository transformerRepository
        )
        {
            Options = options;
            TransformerRepository = transformerRepository;
            _libraryHandle = PlatformLoader.LoadLibrary(path);
            _path = path;
            _interfaceType = interfaceType;
        }

        /// <summary>
        /// Forwards the symbol loading call to the wrapped platform loader.
        /// </summary>
        /// <param name="sym">The symbol name.</param>
        /// <returns>A handle to the symbol</returns>
        [PublicAPI]
        protected IntPtr LoadSymbol([NotNull] string sym) => PlatformLoader.LoadSymbol(_libraryHandle, sym);

        /// <summary>
        /// Forwards the function loading call to the wrapped platform loader.
        /// </summary>
        /// <param name="sym">The symbol name.</param>
        /// <typeparam name="T">The delegate to load the symbol as.</typeparam>
        /// <returns>A function delegate.</returns>
        [PublicAPI]
        protected T LoadFunction<T>([NotNull] string sym) => PlatformLoader.LoadFunction<T>(_libraryHandle, sym);

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

            GC.SuppressFinalize(this);

            IsDisposed = true;

            PlatformLoader.CloseLibrary(_libraryHandle);
            _libraryHandle = IntPtr.Zero;
        }
    }
}
