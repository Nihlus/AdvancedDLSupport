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
            "DYLD_FALLBACK_LIBRARY_PATH",
            "DYLD_FALLBACK_LIBRARY_PATH"
        };

        /// <inheritdoc />
        public string Resolve(string library)
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
                        return libraryLocation;
                    }
                }
            }

            throw new FileNotFoundException("The specified library was not found in any of the loader search paths.");
        }
    }
}
