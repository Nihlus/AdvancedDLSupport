//
//  DllMapResolver.cs
//
//  Author:
//       Jarl Gullberg <jarl.gullberg@gmail.com>
//
//  Copyright (c) Jarl Gullberg
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
using System.IO;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;

namespace Mono.DllMap
{
    /// <summary>
    /// Helper class for resolving library paths and alternate symbol names through Mono's DllMap files.
    /// </summary>
    [PublicAPI]
    public class DllMapResolver
    {
        /// <summary>
        /// Finds the matching remapping entry, if any, for the given library name and type, and returns the
        /// remapped library name. If no match is found, the library name is returned unchanged.
        /// </summary>
        /// <typeparam name="T">A type defined in the assembly to search the DllMap for.</typeparam>
        /// <param name="libraryName">The original name of the library.</param>
        /// <returns>The remapped name.</returns>
        [PublicAPI, Pure, NotNull]
        public string MapLibraryName<T>([NotNull] string libraryName) => MapLibraryName(typeof(T), libraryName);

        /// <summary>
        /// Finds the matching remapping entry, if any, for the given library name and type, and returns the
        /// remapped library name. If no match is found, the library name is returned unchanged.
        /// </summary>
        /// <param name="type">A type defined in the assembly to search the DllMap for.</param>
        /// <param name="libraryName">The original name of the library.</param>
        /// <returns>The remapped name.</returns>
        [PublicAPI, Pure, NotNull]
        public string MapLibraryName([NotNull] Type type, [NotNull] string libraryName) => MapLibraryName(type.Assembly, libraryName);

        /// <summary>
        /// Finds the matching remapping entry, if any, for the given library name and assembly, and returns the
        /// remapped library name. If no match is found, the library name is returned unchanged.
        /// </summary>
        /// <param name="assembly">The assembly to search the DllMap for.</param>
        /// <param name="libraryName">The original name of the library.</param>
        /// <returns>The remapped name.</returns>
        [PublicAPI, Pure, NotNull]
        public string MapLibraryName([NotNull] Assembly assembly, [NotNull] string libraryName)
        {
            if (!HasDllMapFile(assembly))
            {
                return libraryName;
            }

            var map = GetDllMap(assembly);

            return MapLibraryName(map, libraryName);
        }

        /// <summary>
        /// Finds the matching remapping entry, if any, for the given library name, and returns the remapped library
        /// name. If no match is found, the library name is returned unchanged.
        /// </summary>
        /// <param name="configuration">The DllMap to search.</param>
        /// <param name="libraryName">The original name of the library.</param>
        /// <returns>The remapped name.</returns>
        [PublicAPI, Pure, NotNull]
        public string MapLibraryName([NotNull] DllConfiguration configuration, [NotNull] string libraryName)
        {
            var mapEntry = configuration.GetRelevantMaps().FirstOrDefault(m => m.SourceLibrary == libraryName);
            if (mapEntry is null)
            {
                return libraryName;
            }

            return mapEntry.TargetLibrary
            ?? throw new InvalidOperationException
            (
                "The given library had a mapping, but the mapping lacked a target library."
            );
        }

        /// <summary>
        /// Determines whether or not the assembly that the given type is declared in has a Mono DllMap configuration
        /// file.
        /// </summary>
        /// <typeparam name="T">The type to check.</typeparam>
        /// <returns>true if it has a file; otherwise, false.</returns>
        [PublicAPI, Pure]
        public bool HasDllMapFile<T>() => HasDllMapFile(typeof(T));

        /// <summary>
        /// Determines whether or not the assembly that the given type is declared in has a Mono DllMap configuration
        /// file.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <returns>true if it has a file; otherwise, false.</returns>
        [PublicAPI, Pure]
        public bool HasDllMapFile([NotNull] Type type) => HasDllMapFile(type.Assembly);

        /// <summary>
        /// Determines whether or not the given assembly has a Mono DllMap configuration file.
        /// </summary>
        /// <param name="assembly">The assembly to check.</param>
        /// <returns>true if it has a file; otherwise, false.</returns>
        [PublicAPI, Pure]
        public bool HasDllMapFile([NotNull] Assembly assembly)
        {
            var mapPath = GetDllMapPath(assembly);
            return File.Exists(mapPath) && DllConfiguration.TryParse(File.ReadAllText(mapPath), out _);
        }

        /// <summary>
        /// Gets the DllMap file for the assembly that the given type is declared in.
        /// </summary>
        /// <typeparam name="T">The type to get the configuration for.</typeparam>
        /// <returns>The DllMap.</returns>
        [PublicAPI, Pure, NotNull]
        public DllConfiguration GetDllMap<T>() => GetDllMap(typeof(T));

        /// <summary>
        /// Gets the DllMap file for the assembly that the given type is declared in.
        /// </summary>
        /// <param name="type">The type to get the configuration for.</param>
        /// <returns>The DllMap.</returns>
        [PublicAPI, Pure, NotNull]
        public DllConfiguration GetDllMap([NotNull] Type type) => GetDllMap(type.Assembly);

        /// <summary>
        /// Gets the DllMap file for the given assembly.
        /// </summary>
        /// <param name="assembly">The assembly to get the configuration for.</param>
        /// <returns>The DllMap.</returns>
        [PublicAPI, Pure, NotNull]
        public DllConfiguration GetDllMap([NotNull] Assembly assembly)
        {
            var mapPath = GetDllMapPath(assembly);
            if (!File.Exists(mapPath))
            {
                throw new FileNotFoundException("Could not find a DllMap file associated with the assembly.", mapPath);
            }

            return DllConfiguration.Parse(File.ReadAllText(mapPath));
        }

        [Pure, NotNull]
        private string GetDllMapPath([NotNull] Assembly assembly)
        {
            var assemblyName = assembly.GetName().Name;
            var assemblyDirectory = Directory.GetParent(assembly.Location).FullName;
            var assemblyExtension = Path.GetExtension(assembly.Location);

            var mapPath = Path.Combine(assemblyDirectory, $"{assemblyName}{assemblyExtension}.config");
            return mapPath;
        }
    }
}
