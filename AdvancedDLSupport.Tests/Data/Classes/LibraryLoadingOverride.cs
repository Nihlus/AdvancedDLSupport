//
//  LibraryLoadingOverride.cs
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
using AdvancedDLSupport.Loaders;
using JetBrains.Annotations;

#pragma warning disable SA1600, CS1591

namespace AdvancedDLSupport.Tests.Data.Classes
{
    internal class LibraryLoadingOverride : ILibraryLoader
    {
        private readonly ILibraryLoader _defaultLoader;

        public bool LoadLibraryCalled { get; private set; }

        public LibraryLoadingOverride(ILibraryLoader @default)
        {
            _defaultLoader = @default;
        }

        public IntPtr LoadLibrary([CanBeNull] string path)
        {
            LoadLibraryCalled = true;

            return _defaultLoader.LoadLibrary(path);
        }

        public bool CloseLibrary(IntPtr library)
        {
            return _defaultLoader.CloseLibrary(library);
        }
    }
}
