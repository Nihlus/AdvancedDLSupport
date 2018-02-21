//
//  LocalPathResolver.cs
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
using System.IO;
using System.Reflection;

namespace AdvancedDLSupport
{
    /// <summary>
    /// Resolves locally bundled library paths.
    /// </summary>
    internal sealed class LocalPathResolver : ILibraryPathResolver
    {
        private readonly string _executingDirectory;
        private readonly string _libraryDirectory;
        private readonly string _platformLibraryDirectory;

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalPathResolver"/> class.
        /// </summary>
        public LocalPathResolver()
        {
            _executingDirectory = Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName;
            _libraryDirectory = Path.Combine(_executingDirectory, "lib");
            _platformLibraryDirectory = Path.Combine(_libraryDirectory, Environment.Is64BitProcess ? "x64" : "x86");
        }

        /// <inheritdoc />
        public ResolvePathResult Resolve(string library)
        {
            // First, check next to the executable
            var libraryLocation = Path.GetFullPath(Path.Combine(_executingDirectory, library));
            if (File.Exists(libraryLocation))
            {
                return ResolvePathResult.FromSuccess(libraryLocation);
            }

            // Check the local library directory
            libraryLocation = Path.GetFullPath(Path.Combine(_libraryDirectory, library));
            if (File.Exists(libraryLocation))
            {
                return ResolvePathResult.FromSuccess(libraryLocation);
            }

            // Check platform-specific directory
            libraryLocation = Path.GetFullPath(Path.Combine(_platformLibraryDirectory, library));
            if (File.Exists(libraryLocation))
            {
                return ResolvePathResult.FromSuccess(libraryLocation);
            }

            return ResolvePathResult.FromError(new FileNotFoundException("No local copy of the given library could be found.", library));
        }
    }
}
