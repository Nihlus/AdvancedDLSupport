//
//  WindowsPathResolver.cs
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
using System.Linq;
using System.Reflection;
using AdvancedDLSupport.Extensions;

namespace AdvancedDLSupport
{
    /// <summary>
    /// Resolves library paths on Windows.
    /// </summary>
    internal sealed class WindowsPathResolver : ILibraryPathResolver
    {
        /// <inheritdoc />
        public ResolvePathResult Resolve(string library)
        {
            var executingDir = Directory.GetParent(Assembly.GetEntryAssembly().Location).FullName;
            var libraryLocation = Path.GetFullPath(Path.Combine(executingDir, library));
            if (File.Exists(libraryLocation))
            {
                return ResolvePathResult.FromSuccess(libraryLocation);
            }

            var sysDir = Environment.SystemDirectory;
            libraryLocation = Path.GetFullPath(Path.Combine(sysDir, library));
            if (File.Exists(libraryLocation))
            {
                return ResolvePathResult.FromSuccess(libraryLocation);
            }

            var windowsDir = Environment.GetFolderPath(Environment.SpecialFolder.Windows);

            var sys16Dir = Path.Combine(windowsDir, "System");
            libraryLocation = Path.GetFullPath(Path.Combine(sys16Dir, library));
            if (File.Exists(libraryLocation))
            {
                return ResolvePathResult.FromSuccess(libraryLocation);
            }

            libraryLocation = Path.GetFullPath(Path.Combine(windowsDir, library));
            if (File.Exists(libraryLocation))
            {
                return ResolvePathResult.FromSuccess(libraryLocation);
            }

            var currentDir = Directory.GetCurrentDirectory();
            libraryLocation = Path.GetFullPath(Path.Combine(currentDir, library));
            if (File.Exists(libraryLocation))
            {
                return ResolvePathResult.FromSuccess(libraryLocation);
            }

            var pathDirs = Environment.GetEnvironmentVariable("PATH").Split(';').Where(p => !p.IsNullOrWhiteSpace());
            foreach (var path in pathDirs)
            {
                libraryLocation = Path.GetFullPath(Path.Combine(path, library));
                if (File.Exists(libraryLocation))
                {
                    return ResolvePathResult.FromSuccess(libraryLocation);
                }
            }

            return ResolvePathResult.FromError(new FileNotFoundException("The specified library was not found in any of the loader search paths.", library));
        }
    }
}
