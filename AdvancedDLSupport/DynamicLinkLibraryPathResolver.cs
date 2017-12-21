using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace AdvancedDLSupport
{
    /// <summary>
    /// Resolves dynamic link library paths.
    /// </summary>
    public class DynamicLinkLibraryPathResolver
    {
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
        public static string ResolveAbsolutePath(string library, bool localFirst)
        {
            var candidates = GenerateLibraryCandidates(library);
            foreach (var candiate in candidates)
            {
                try
                {
                    var result = ResolveCandidate(candiate, localFirst);
                    return result;
                }
                catch (FileNotFoundException)
                {
                }
            }

            throw new FileNotFoundException("The specified library was not found in any of the loader search paths.");
        }

        private static string ResolveCandidate(string candidate, bool localFirst)
        {
            if (Path.IsPathRooted(candidate) && File.Exists(candidate))
            {
                return Path.GetFullPath(candidate);
            }

            if (localFirst)
            {
                var executingDir = Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName;
                var libraryLocation = Path.GetFullPath(Path.Combine(executingDir, candidate));
                if (File.Exists(libraryLocation))
                {
                    return libraryLocation;
                }
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return ResolveWindowsPath(candidate);
            }

            /*
                Temporary hack until BSD is added to RuntimeInformation. OSDescription should contain the output from
                "uname -srv", which will report something along the lines of FreeBSD or OpenBSD plus some more info.
            */
            bool isBSD = RuntimeInformation.OSDescription.ToUpperInvariant().Contains("BSD");
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || isBSD)
            {
                return ResolveLinuxPath(candidate);
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return ResolveMacOSPath(candidate);
            }

            throw new PlatformNotSupportedException($"Cannot resolve linker paths on this platform: {RuntimeInformation.OSDescription}");
        }

        private static IEnumerable<string> GenerateLibraryCandidates(string library)
        {
            var candidates = new List<string>();
            candidates.Add(library);

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && !library.EndsWith(".dll"))
            {
                candidates.Add($"{library}.dll");
            }

            /*
                Temporary hack until BSD is added to RuntimeInformation. OSDescription should contain the output from
                "uname -srv", which will report something along the lines of FreeBSD or OpenBSD plus some more info.
            */
            bool isBSD = RuntimeInformation.OSDescription.ToUpperInvariant().Contains("BSD");
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || isBSD)
            {
                var noSuffix = !library.EndsWith(".so");
                var noPrefix = !Path.GetFileName(library).StartsWith("lib");
                if (noSuffix)
                {
                    candidates.Add($"{library}.so");
                }

                if (noPrefix)
                {
                    candidates.Add($"lib{library}");
                }

                if (noPrefix && noSuffix)
                {
                    candidates.Add($"lib{library}.so");
                }
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX) && !library.EndsWith(".dylib"))
            {
                candidates.Add($"{library}.dylib");
            }

            return candidates;
        }

        private static string ResolveMacOSPath(string library)
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

        private static string ResolveLinuxPath(string library)
        {
            var libraryPaths = Environment.GetEnvironmentVariable("LD_LIBRARY_PATH").Split(';');

            string libraryLocation;
            foreach (var path in libraryPaths)
            {
                libraryLocation = Path.GetFullPath(Path.Combine(path, library));
                if (File.Exists(libraryLocation))
                {
                    return libraryLocation;
                }
            }

            if (File.Exists("/etc/ld.so.cache"))
            {
                var cachedLibraries = File.ReadAllText("/etc/ld.so.cache").Split('\0');
                var cachedMatch = cachedLibraries.FirstOrDefault(l => l.EndsWith(library));
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

        private static string ResolveWindowsPath(string library)
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

            var pathDirs = Environment.GetEnvironmentVariable("PATH").Split(';');
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
