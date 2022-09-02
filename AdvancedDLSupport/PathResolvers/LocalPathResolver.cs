//
//  LocalPathResolver.cs
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
using System.IO;
using System.Reflection;

namespace AdvancedDLSupport;

/// <summary>
/// Resolves locally bundled library paths.
/// </summary>
internal sealed class LocalPathResolver : ILibraryPathResolver
{
    private readonly string? _entryAssemblyDirectory;

    private readonly string? _executingAssemblyDirectory;

    private readonly string? _currentDirectory;

    /// <summary>
    /// Initializes a new instance of the <see cref="LocalPathResolver"/> class.
    /// </summary>
    public LocalPathResolver()
    {
        var entryAssembly = Assembly.GetEntryAssembly();
        _entryAssemblyDirectory = entryAssembly is null
            ? null
            : Directory.GetParent(entryAssembly.Location)?.FullName;

        _executingAssemblyDirectory = Directory.GetParent(Assembly.GetExecutingAssembly().Location)?.FullName;

        _currentDirectory = Directory.GetCurrentDirectory();
    }

    /// <inheritdoc />
    public ResolvePathResult Resolve(string library)
    {
        // First, check next to the entry executable
        if (!(_entryAssemblyDirectory is null))
        {
            var entryResult = ScanPathForLibrary(_entryAssemblyDirectory, library);
            if (entryResult.IsSuccess)
            {
                return entryResult;
            }
        }

        if (!(_executingAssemblyDirectory is null))
        {
            var executingResult = ScanPathForLibrary(_executingAssemblyDirectory, library);
            if (executingResult.IsSuccess)
            {
                return executingResult;
            }
        }

        // Then, check the current directory
        if (_currentDirectory is null)
        {
            return ResolvePathResult.FromError
                (new FileNotFoundException("No local copy of the given library could be found.", library));
        }

        var currentResult = ScanPathForLibrary(_currentDirectory, library);
        if (currentResult.IsSuccess)
        {
            return currentResult;
        }

        return ResolvePathResult.FromError(new FileNotFoundException("No local copy of the given library could be found.", library));
    }

    private ResolvePathResult ScanPathForLibrary(string path, string library)
    {
        var libraryLocation = Path.GetFullPath(Path.Combine(path, library));
        if (File.Exists(libraryLocation))
        {
            return ResolvePathResult.FromSuccess(libraryLocation);
        }

        // Check the local library directory
        libraryLocation = Path.GetFullPath(Path.Combine(path, "lib", library));
        if (File.Exists(libraryLocation))
        {
            return ResolvePathResult.FromSuccess(libraryLocation);
        }

        // Check platform-specific directory
        var bitness = Environment.Is64BitProcess ? "x64" : "x86";
        libraryLocation = Path.GetFullPath(Path.Combine(path, "lib", bitness, library));
        if (File.Exists(libraryLocation))
        {
            return ResolvePathResult.FromSuccess(libraryLocation);
        }

        return ResolvePathResult.FromError(new FileNotFoundException("No local copy of the given library could be found.", library));
    }
}
