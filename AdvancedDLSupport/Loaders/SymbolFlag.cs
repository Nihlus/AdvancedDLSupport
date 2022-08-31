//
//  SymbolFlag.cs
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

// ReSharper disable InconsistentNaming
using System;
using JetBrains.Annotations;

namespace AdvancedDLSupport.Loaders;

/// <summary>
/// <see cref="dl.open"/> flags. Taken from the source code of GNU libc.
///
/// <a href="https://github.com/lattera/glibc/blob/master/bits/dlfcn.h"/>
/// </summary>
[PublicAPI, Flags]
public enum SymbolFlag
{
    /// <summary>
    /// The default flags.
    /// </summary>
    RTLD_DEFAULT = RTLD_NOW,

    /// <summary>
    /// Lazy function call binding.
    /// </summary>
    RTLD_LAZY = 0x00001,

    /// <summary>
    /// Immediate function call binding.
    /// </summary>
    RTLD_NOW = 0x00002,

    /// <summary>
    /// If set, makes the symbols of the loaded object and its dependencies visible
    /// as if the object was linked directly into the program.
    /// </summary>
    RTLD_GLOBAL = 0x00100,

    /// <summary>
    /// The inverse of <see cref="RTLD_GLOBAL"/>. Typically, this is the default behaviour.
    /// </summary>
    RTLD_LOCAL = 0x00000,

    /// <summary>
    /// Do not delete the object when closed.
    /// </summary>
    RTLD_NODELETE = 0x01000
}
