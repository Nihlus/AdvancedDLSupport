using System;
using System.IO;

namespace AdvancedDLSupport
{
    /// <summary>
    /// Resolves library paths on macOS.
    /// </summary>
    public class MacOSPathResolver : ILibraryPathResolver
    {
        /// <inheritdoc />
        public string Resolve(string library)
        {
            var libraryPaths = Environment.GetEnvironmentVariable("DYLD_FRAMEWORK_PATH").Split(':');

            string libraryLocation;
            foreach (var path in libraryPaths)
            {
                libraryLocation = Path.GetFullPath(Path.Combine(path, library));
                if (File.Exists(libraryLocation))
                {
                    return libraryLocation;
                }
            }

            libraryPaths = Environment.GetEnvironmentVariable("DYLD_LIBRARY_PATH").Split(':');
            foreach (var path in libraryPaths)
            {
                libraryLocation = Path.GetFullPath(Path.Combine(path, library));
                if (File.Exists(libraryLocation))
                {
                    return libraryLocation;
                }
            }

            libraryPaths = Environment.GetEnvironmentVariable("DYLD_FALLBACK_FRAMEWORK_PATH").Split(':');
            foreach (var path in libraryPaths)
            {
                libraryLocation = Path.GetFullPath(Path.Combine(path, library));
                if (File.Exists(libraryLocation))
                {
                    return libraryLocation;
                }
            }

            libraryPaths = Environment.GetEnvironmentVariable("DYLD_FALLBACK_LIBRARY_PATH").Split(':');
            foreach (var path in libraryPaths)
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
