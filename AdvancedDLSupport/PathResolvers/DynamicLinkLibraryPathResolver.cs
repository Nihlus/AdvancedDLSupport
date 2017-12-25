using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace AdvancedDLSupport
{
    /// <summary>
    /// Resolves dynamic link library paths.
    /// </summary>
    internal static class DynamicLinkLibraryPathResolver
    {
        [NotNull]
        private static readonly ILibraryPathResolver PathResolver;

        static DynamicLinkLibraryPathResolver()
        {
            PathResolver = SelectPathResolver();
        }

        [NotNull]
        private static ILibraryPathResolver SelectPathResolver()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return new WindowsPathResolver();
            }

            /*
                Temporary hack until BSD is added to RuntimeInformation. OSDescription should contain the output from
                "uname -srv", which will report something along the lines of FreeBSD or OpenBSD plus some more info.
            */
            bool isBSD = RuntimeInformation.OSDescription.ToUpperInvariant().Contains("BSD");
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || isBSD)
            {
                return new LinuxPathResolver();
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return new MacOSPathResolver();
            }

            throw new PlatformNotSupportedException($"Cannot resolve linker paths on this platform: {RuntimeInformation.OSDescription}");
        }

        /// <summary>
        /// Resolves the absolute path to the given library.
        /// </summary>
        /// <param name="library">The name or path of the library to load.</param>
        /// <param name="localFirst">Whether or not the executable's local directory should be searched first.</param>
        /// <returns>The absolute path to the library.</returns>
        /// <exception cref="PlatformNotSupportedException">
        /// Thrown if the current platform doesn't have a path
        /// resolver defined.</exception>
        /// <exception cref="FileNotFoundException">Thrown if no library file can be found.</exception>
        public static ResolvePathResult ResolveAbsolutePath([NotNull] string library, bool localFirst)
        {
            var candidates = GenerateLibraryCandidates(library).ToList();

            if (localFirst)
            {
                foreach (var candidate in candidates)
                {
                    var executingDir = Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName;
                    var libraryLocation = Path.GetFullPath(Path.Combine(executingDir, candidate));
                    if (File.Exists(libraryLocation))
                    {
                        return ResolvePathResult.FromSuccess(libraryLocation);
                    }
                }
            }

            foreach (var candidate in candidates)
            {
                if (Path.IsPathRooted(candidate) && File.Exists(candidate))
                {
                    return ResolvePathResult.FromSuccess(Path.GetFullPath(candidate));
                }

                try
                {
                    var result = PathResolver.Resolve(candidate);
                    return ResolvePathResult.FromSuccess(result);
                }
                catch (FileNotFoundException)
                {
                }
            }

            return ResolvePathResult.FromError(new FileNotFoundException("The specified library was not found in any of the loader search paths.", library));
        }

        [NotNull, ItemNotNull]
        private static IEnumerable<string> GenerateLibraryCandidates([NotNull] string library)
        {
            var candidates = new List<string>();
            candidates.Add(library);

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && !library.EndsWith(".dll"))
            {
                candidates.Add($"{library}.dll");
            }

            bool isBSD = RuntimeInformation.OSDescription.ToUpperInvariant().Contains("BSD");
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || isBSD || RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                var prefix = "lib";
                var suffix = RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? ".dylib" : ".so";

                var noSuffix = !library.EndsWith(suffix);
                var noPrefix = !Path.GetFileName(library).StartsWith(prefix);
                if (noSuffix)
                {
                    candidates.Add($"{library}{suffix}");
                }

                if (noPrefix)
                {
                    candidates.Add($"{prefix}{library}");
                }

                if (noPrefix && noSuffix)
                {
                    candidates.Add($"{prefix}{library}{suffix}");
                }
            }

            return candidates;
        }
    }
}
