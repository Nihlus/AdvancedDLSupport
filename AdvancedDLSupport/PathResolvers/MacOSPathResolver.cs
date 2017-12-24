using System;
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
        /// <inheritdoc />
        public string Resolve(string library)
        {
            var libraryPaths = Environment.GetEnvironmentVariable("DYLD_FRAMEWORK_PATH").Split(':').Where(p => !p.IsNullOrWhiteSpace());

            string libraryLocation;
            foreach (var path in libraryPaths)
            {
                libraryLocation = Path.GetFullPath(Path.Combine(path, library));
                if (File.Exists(libraryLocation))
                {
                    return libraryLocation;
                }
            }

            libraryPaths = Environment.GetEnvironmentVariable("DYLD_LIBRARY_PATH").Split(':').Where(p => !p.IsNullOrWhiteSpace());
            foreach (var path in libraryPaths)
            {
                libraryLocation = Path.GetFullPath(Path.Combine(path, library));
                if (File.Exists(libraryLocation))
                {
                    return libraryLocation;
                }
            }

            libraryPaths = Environment.GetEnvironmentVariable("DYLD_FALLBACK_FRAMEWORK_PATH").Split(':').Where(p => !p.IsNullOrWhiteSpace());
            foreach (var path in libraryPaths)
            {
                libraryLocation = Path.GetFullPath(Path.Combine(path, library));
                if (File.Exists(libraryLocation))
                {
                    return libraryLocation;
                }
            }

            libraryPaths = Environment.GetEnvironmentVariable("DYLD_FALLBACK_LIBRARY_PATH").Split(':').Where(p => !p.IsNullOrWhiteSpace());
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
