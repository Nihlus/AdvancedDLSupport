using System;
using System.IO;
using System.Reflection;

namespace AdvancedDLSupport
{
    /// <summary>
    /// Resolves locally bundled library paths.
    /// </summary>
    internal class LocalPathResolver : ILibraryPathResolver
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
