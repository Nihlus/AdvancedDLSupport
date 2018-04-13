//
//  PlatformLoaderBase.cs
//
//  Copyright (c) 2018 Firwood Software
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.
//

using System;
using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace AdvancedDLSupport.Loaders
{
    /// <summary>
    /// Acts as the base for platform loaders.
    /// </summary>
    [PublicAPI]
    public abstract class PlatformLoaderBase : IPlatformLoader
    {
        /// <inheritdoc />
        public T LoadFunction<T>(IntPtr library, string symbolName) =>
            Marshal.GetDelegateForFunctionPointer<T>(LoadSymbol(library, symbolName));

        /// <inheritdoc />
        public IntPtr LoadLibrary(string path) => LoadLibraryInternal(path);

        /// <summary>
        /// Load the given library.
        /// </summary>
        /// <param name="path">The path to the library.</param>
        /// <returns>A handle to the library. This value carries no intrinsic meaning.</returns>
        /// <exception cref="LibraryLoadingException">Thrown if the library could not be loaded.</exception>
        protected abstract IntPtr LoadLibraryInternal([CanBeNull] string path);

        /// <inheritdoc />
        [Pure]
        public abstract IntPtr LoadSymbol(IntPtr library, string symbolName);

        /// <inheritdoc />
        public abstract bool CloseLibrary(IntPtr library);

        /// <summary>
        /// Selects the appropriate platform loader based on the current platform.
        /// </summary>
        /// <returns>A platform loader for the current platform..</returns>
        /// <exception cref="PlatformNotSupportedException">Thrown if the current platform is not supported.</exception>
        [PublicAPI, NotNull, Pure]
        public static IPlatformLoader SelectPlatformLoader()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return new WindowsPlatformLoader();
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return new LinuxPlatformLoader();
            }

            /*
                Temporary hack until BSD is added to RuntimeInformation. OSDescription should contain the output from
                "uname -srv", which will report something along the lines of FreeBSD or OpenBSD plus some more info.
            */
            bool isBSD = RuntimeInformation.OSDescription.ToUpperInvariant().Contains("BSD");
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX) || isBSD)
            {
                return new BSDPlatformLoader();
            }

            throw new PlatformNotSupportedException($"Cannot load native libraries on this platform: {RuntimeInformation.OSDescription}");
        }
    }
}
