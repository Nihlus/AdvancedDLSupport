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
        public string Resolve(string library)
        {
            var executingDir = Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName;
            var libraryLocation = Path.GetFullPath(Path.Combine(executingDir, library));
            if (File.Exists(libraryLocation))
            {
                return libraryLocation;
            }

            var windowsDir = Environment.GetFolderPath(Environment.SpecialFolder.Windows);

            var sys32Dir = Path.Combine(windowsDir, "System32");
            libraryLocation = Path.GetFullPath(Path.Combine(sys32Dir, library));
            if (File.Exists(libraryLocation))
            {
                return libraryLocation;
            }

            var sys16Dir = Path.Combine(windowsDir, "System");
            libraryLocation = Path.GetFullPath(Path.Combine(sys16Dir, library));
            if (File.Exists(libraryLocation))
            {
                return libraryLocation;
            }

            libraryLocation = Path.GetFullPath(Path.Combine(windowsDir, library));
            if (File.Exists(libraryLocation))
            {
                return libraryLocation;
            }

            var currentDir = Directory.GetCurrentDirectory();
            libraryLocation = Path.GetFullPath(Path.Combine(currentDir, library));
            if (File.Exists(libraryLocation))
            {
                return libraryLocation;
            }

            var pathDirs = Environment.GetEnvironmentVariable("PATH").Split(';').Where(p => !p.IsNullOrWhiteSpace());
            foreach (var path in pathDirs)
            {
                libraryLocation = Path.GetFullPath(Path.Combine(path, library));
                if (File.Exists(libraryLocation))
                {
                    return libraryLocation;
                }
            }

            throw new FileNotFoundException("The specified library was not found in any of the loader search paths.");
        }
    }
}
