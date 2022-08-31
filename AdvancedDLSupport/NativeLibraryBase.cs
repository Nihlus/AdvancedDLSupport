//
//  NativeLibraryBase.cs
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
using System.Collections.Generic;
using System.Runtime.InteropServices;
using AdvancedDLSupport.Loaders;
using JetBrains.Annotations;
using Mono.DllMap.Extensions;
using static AdvancedDLSupport.ImplementationOptions;

namespace AdvancedDLSupport;

/// <summary>
/// Internal base class for library implementations.
/// </summary>
[PublicAPI]
public abstract class NativeLibraryBase : IDisposable
{
    /// <summary>
    /// Delegate cache storage to keep delegates alive.
    /// </summary>
    private HashSet<Delegate> _delegateStorage = new HashSet<Delegate>();

    /// <summary>
    /// Gets a value indicating whether or not the library has been disposed.
    /// </summary>
    [PublicAPI]
    public bool IsDisposed { get; private set; }

    /// <summary>
    /// Gets or sets the set of options that were used to construct the type.
    /// </summary>
    internal ImplementationOptions Options { get; set; }

    private readonly ILibraryLoader _libraryLoader;
    private readonly ISymbolLoader _symbolLoader;

    /// <summary>
    /// Gets an opaque native handle to the library.
    /// </summary>
    private IntPtr _libraryHandle;

    /// <summary>
    /// Initializes a new instance of the <see cref="NativeLibraryBase"/> class.
    /// </summary>
    /// <param name="path">The path to the library.</param>
    /// <param name="options">Whether or not this library can be disposed.</param>
    /// <param name="libLoader">Overriding library loader.</param>
    /// <param name="symLoader">Overriding symbol loader.</param>
    [PublicAPI, AnonymousConstructor]
    protected NativeLibraryBase
    (
        string? path,
        ImplementationOptions options,
        ILibraryLoader? libLoader = null,
        ISymbolLoader? symLoader = null
    )
    {
        _libraryLoader = libLoader ?? PlatformLoaderBase.PlatformLoader;
        _symbolLoader = symLoader ?? PlatformLoaderBase.PlatformLoader;
        Options = options;
        _libraryHandle = _libraryLoader.LoadLibrary(path);
    }

    /// <summary>
    /// Forwards the symbol loading call to the wrapped platform loader.
    /// </summary>
    /// <param name="sym">The symbol name.</param>
    /// <returns>A handle to the symbol.</returns>
    internal IntPtr LoadSymbol(string sym) => _symbolLoader.LoadSymbol(_libraryHandle, sym);

    /// <summary>
    /// Forwards the function loading call to the wrapped platform loader.
    /// </summary>
    /// <param name="sym">The symbol name.</param>
    /// <typeparam name="T">The delegate to load the symbol as.</typeparam>
    /// <returns>A function delegate.</returns>
    [NotNull]
    internal T LoadFunction<T>(string sym) => Marshal.GetDelegateForFunctionPointer<T>(LoadSymbol(sym));

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

    /// <summary>
    /// Adds a delegate to keep its lifetime until this library gets disposed.
    /// </summary>
    /// <param name="del">The delegate to keep alive.</param>
    protected void AddLifetimeDelegate(Delegate del)
    {
        _delegateStorage.Add(del);
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

        _delegateStorage.Clear();

        _libraryLoader.CloseLibrary(_libraryHandle);
        _libraryHandle = IntPtr.Zero;
    }
}
