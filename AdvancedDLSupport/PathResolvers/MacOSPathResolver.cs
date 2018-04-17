//
//  MacOSPathResolver.cs
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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AdvancedDLSupport.Extensions;

namespace AdvancedDLSupport
{
    /// <summary>
    /// Resolves library paths on macOS.
    /// </summary>
    internal sealed class MacOSPathResolver : ILibraryPathResolver
    {
        private static readonly IReadOnlyList<string> EnvironmentVariables = new[]
        {
            "DYLD_FRAMEWORK_PATH",
            "DYLD_LIBRARY_PATH",
            "DYLD_FALLBACK_FRAMEWORK_PATH",
            "DYLD_FALLBACK_LIBRARY_PATH"
        };

        /// <inheritdoc />
        public ResolvePathResult Resolve(string library)
        {
            foreach (var variable in EnvironmentVariables)
            {
                var libraryPaths = Environment.GetEnvironmentVariable(variable)?.Split(':').Where(p => !p.IsNullOrWhiteSpace());

                if (libraryPaths is null)
                {
                    continue;
                }

                foreach (var path in libraryPaths)
                {
                    var libraryLocation = Path.GetFullPath(Path.Combine(path, library));
                    if (File.Exists(libraryLocation))
                    {
                        return ResolvePathResult.FromSuccess(libraryLocation);
                    }
                }
            }

            return ResolvePathResult.FromError(new FileNotFoundException("The specified library was not found in any of the loader search paths.", library));
        }
    }
}
