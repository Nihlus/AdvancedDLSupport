//
//  LinuxPathResolver.cs
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
using System.Linq;
using AdvancedDLSupport.Extensions;

namespace AdvancedDLSupport
{
    /// <summary>
    /// Resolves library paths on Linux (and other unix-like systems).
    /// </summary>
    internal sealed class LinuxPathResolver : ILibraryPathResolver
    {
        /// <inheritdoc />
        public ResolvePathResult Resolve(string library)
        {
            var libraryPaths = Environment.GetEnvironmentVariable("LD_LIBRARY_PATH")?.Split(':').Where(p => !p.IsNullOrWhiteSpace());

            string libraryLocation;

            if (!(libraryPaths is null))
            {
                foreach (var path in libraryPaths)
                {
                    libraryLocation = Path.GetFullPath(Path.Combine(path, library));
                    if (File.Exists(libraryLocation))
                    {
                        return ResolvePathResult.FromSuccess(libraryLocation);
                    }
                }
            }

            if (File.Exists("/etc/ld.so.cache"))
            {
                var cachedLibraries = File.ReadAllText("/etc/ld.so.cache").Split('\0');
                var cachedMatch = cachedLibraries.FirstOrDefault
                (
                    l =>
                        l.EndsWith(library) &&
                        Path.GetFileName(l) == Path.GetFileName(library)
                );

                if (!(cachedMatch is null))
                {
                    return ResolvePathResult.FromSuccess(cachedMatch);
                }
            }

            libraryLocation = Path.GetFullPath(Path.Combine("/lib", library));
            if (File.Exists(libraryLocation))
            {
                return ResolvePathResult.FromSuccess(libraryLocation);
            }

            libraryLocation = Path.GetFullPath(Path.Combine("/usr/lib", library));
            if (File.Exists(libraryLocation))
            {
                return ResolvePathResult.FromSuccess(libraryLocation);
            }

            return ResolvePathResult.FromError(new FileNotFoundException("The specified library was not found in any of the loader search paths.", library));
        }
    }
}
