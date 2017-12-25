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
        public string Resolve(string library)
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
                        return libraryLocation;
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
                    return cachedMatch;
                }
            }

            libraryLocation = Path.GetFullPath(Path.Combine("/lib", library));
            if (File.Exists(libraryLocation))
            {
                return libraryLocation;
            }

            libraryLocation = Path.GetFullPath(Path.Combine("/usr/lib", library));
            if (File.Exists(libraryLocation))
            {
                return libraryLocation;
            }

            if (!(Type.GetType("Mono.Runtime") is null) && library == "__Internal")
            {
                // Mono extension: Search the main program
                return null;
            }

            throw new FileNotFoundException("The specified library was not found in any of the loader search paths.");
        }
    }
}
