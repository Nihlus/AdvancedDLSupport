//
//  DynamicLinkLibraryPathResolver.cs
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
using System.Runtime.InteropServices;
using AdvancedDLSupport.Extensions;
using JetBrains.Annotations;

namespace AdvancedDLSupport
{
    /// <summary>
    /// Resolves dynamic link library paths.
    /// </summary>
    internal class DynamicLinkLibraryPathResolver : ILibraryPathResolver
    {
        [NotNull]
        private static readonly ILibraryPathResolver LocalPathResolver;

        [NotNull]
        private static readonly ILibraryPathResolver PathResolver;

        private bool SearchLocalFirst { get; }

        static DynamicLinkLibraryPathResolver()
        {
            LocalPathResolver = new LocalPathResolver();
            PathResolver = SelectPathResolver();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicLinkLibraryPathResolver"/> class.
        /// </summary>
        /// <param name="searchLocalFirst">Whether or not the local search pattern should be followed first.</param>
        public DynamicLinkLibraryPathResolver(bool searchLocalFirst = true)
        {
            SearchLocalFirst = searchLocalFirst;
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

        /// <inheritdoc />
        public ResolvePathResult Resolve(string library)
        {
            return ResolveAbsolutePath(library, SearchLocalFirst);
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
        private ResolvePathResult ResolveAbsolutePath([NotNull] string library, bool localFirst)
        {
            var candidates = GenerateLibraryCandidates(library).ToList();

            if (library.IsValidPath())
            {
                foreach (var candidate in candidates)
                {
                    if (File.Exists(candidate))
                    {
                        return ResolvePathResult.FromSuccess(Path.GetFullPath(candidate));
                    }
                }
            }

            if (localFirst)
            {
                foreach (var candidate in candidates)
                {
                    var result = LocalPathResolver.Resolve(candidate);
                    if (result.IsSuccess)
                    {
                        return result;
                    }
                }
            }

            foreach (var candidate in candidates)
            {
                var result = PathResolver.Resolve(candidate);
                if (result.IsSuccess)
                {
                    return result;
                }
            }

            if (!(Type.GetType("Mono.Runtime") is null) && library == "__Internal")
            {
                // Mono extension: Search the main program
                return ResolvePathResult.FromSuccess(null);
            }

            return ResolvePathResult.FromError(new FileNotFoundException("The specified library was not found in any of the loader search paths.", library));
        }

        /// <summary>
        /// Generates a set of platform-specific candidate library names.
        /// </summary>
        /// <param name="library">The library name to generate candidates for.</param>
        /// <returns>A list of candidates.</returns>
        [NotNull, ItemNotNull]
        private static IEnumerable<string> GenerateLibraryCandidates([NotNull] string library)
        {
            bool doesLibraryContainPath = false;
            var parentDirectory = Path.GetDirectoryName(library) ?? string.Empty;
            if (library.IsValidPath())
            {
                library = Path.GetFileName(library);
                doesLibraryContainPath = true;
            }

            var candidates = new List<string>
            {
                library
            };

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && !library.EndsWith(".dll"))
            {
                candidates.AddRange(GenerateWindowsCandidates(library));
            }

            bool isBSD = RuntimeInformation.OSDescription.ToUpperInvariant().Contains("BSD");
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || isBSD || RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                candidates.AddRange(GenerateUnixCandidates(library));
            }

            // If we have a parent path we're looking at, mutate the candidate list to include the parent path
            if (doesLibraryContainPath)
            {
                candidates = candidates.Select(c => Path.Combine(parentDirectory, c)).ToList();
            }

            return candidates;
        }

        [NotNull, ItemNotNull]
        private static IEnumerable<string> GenerateWindowsCandidates([NotNull] string library)
        {
            yield return $"{library}.dll";
        }

        [NotNull, ItemNotNull]
        private static IEnumerable<string> GenerateUnixCandidates([NotNull] string library)
        {
            const string prefix = "lib";
            var suffix = RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? ".dylib" : ".so";

            var noSuffix = !library.EndsWith(suffix);
            var noPrefix = !Path.GetFileName(library).StartsWith(prefix);
            if (noSuffix)
            {
                yield return $"{library}{suffix}";
            }

            if (noPrefix)
            {
                yield return $"{prefix}{library}";
            }

            if (noPrefix && noSuffix)
            {
                yield return $"{prefix}{library}{suffix}";
            }
        }
    }
}
