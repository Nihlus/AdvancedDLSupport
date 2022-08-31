//
//  UnixPlatformLoader.cs
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
using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace AdvancedDLSupport.Loaders;

/// <summary>
/// Base class for Unix-family platform loaders, using the dl library.
/// </summary>
internal abstract class UnixPlatformLoader : PlatformLoaderBase
{
    /// <summary>
    /// Gets a value indicating whether or not the dl methods should be loaded from the C library.
    /// </summary>
    protected abstract bool UseCLibrary { get; }

    private readonly Action _resetErrorAction;

    private readonly Func<string, SymbolFlag, IntPtr> _openLibraryFunc;

    private readonly Func<IntPtr, string, IntPtr> _openSymbolFunc;

    private readonly Func<IntPtr, int> _closeLibraryFunc;

    private readonly Func<IntPtr> _getErrorFunc;

    /// <summary>
    /// Initializes a new instance of the <see cref="UnixPlatformLoader"/> class.
    /// </summary>
    protected UnixPlatformLoader()
    {
        _resetErrorAction = () => dl.ResetError(UseCLibrary);
        _openLibraryFunc = (s, flags) => dl.open(s, flags, UseCLibrary);
        _openSymbolFunc = (ptr, s) => dl.sym(ptr, s, UseCLibrary);
        _closeLibraryFunc = ptr => dl.close(ptr, UseCLibrary);
        _getErrorFunc = () => dl.error(UseCLibrary);
    }

    /// <summary>
    /// Load the given library with the given flags.
    /// </summary>
    /// <param name="path">The path to the library.</param>
    /// <param name="flags">The loading flags to use.</param>
    /// <returns>A handle to the library. This value carries no intrinsic meaning.</returns>
    /// <exception cref="LibraryLoadingException">Thrown if the library could not be loaded.</exception>
    private IntPtr LoadLibrary(string? path, SymbolFlag flags)
    {
        _resetErrorAction();

        var libraryHandle = _openLibraryFunc(path, flags);
        if (libraryHandle != IntPtr.Zero)
        {
            return libraryHandle;
        }

        var errorPtr = _getErrorFunc();
        if (errorPtr == IntPtr.Zero)
        {
            throw new LibraryLoadingException("Library could not be loaded, and error information from dl library could not be found.", path);
        }

        throw new LibraryLoadingException(string.Format("Library could not be loaded: {0}", Marshal.PtrToStringAnsi(errorPtr)), path);
    }

    /// <inheritdoc />
    protected override IntPtr LoadLibraryInternal(string path) => LoadLibrary(path, SymbolFlag.RTLD_DEFAULT);

    /// <inheritdoc />
    public override IntPtr LoadSymbol(IntPtr library, string symbolName)
    {
        _resetErrorAction();

        var symbolHandle = _openSymbolFunc(library, symbolName);
        if (symbolHandle != IntPtr.Zero)
        {
            return symbolHandle;
        }

        var errorPtr = _getErrorFunc();
        if (errorPtr == IntPtr.Zero)
        {
            throw new SymbolLoadingException($"Symbol could not be loaded, and error information from dl could not be found. Symbol name: {symbolName}", symbolName);
        }

        var msg = Marshal.PtrToStringAnsi(errorPtr);
        throw new SymbolLoadingException($"Symbol could not be loaded: {msg}. Symbol name: {symbolName}", symbolName);
    }

    /// <inheritdoc />
    public override bool CloseLibrary(IntPtr library)
    {
        return _closeLibraryFunc(library) <= 0;
    }
}
