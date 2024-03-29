﻿//
//  ImplementationOptions.cs
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
using JetBrains.Annotations;

namespace AdvancedDLSupport;

/// <summary>
/// Holds generated implementation flag options.
/// </summary>
[PublicAPI, Flags]
public enum ImplementationOptions
{
    /// <summary>
    /// Generate the bindings with lazy loaded symbol resolution.
    /// </summary>
    [PublicAPI]
    UseLazyBinding = 1 << 0,

    /// <summary>
    /// Generate disposal checks for all binder methods.
    /// </summary>
    [PublicAPI]
    GenerateDisposalChecks = 1 << 1,

    /// <summary>
    /// Enable Mono dllmap support for library scanning.
    /// </summary>
    [PublicAPI]
    EnableDllMapSupport = 1 << 2,

    /// <summary>
    /// Enables use of the `calli` opcode.
    /// </summary>
    [PublicAPI]
    UseIndirectCalls = 1 << 3,

    /// <summary>
    /// Enables code optimizations for the generated assembly.
    /// </summary>
    [PublicAPI]
    EnableOptimizations = 1 << 4,

    /// <summary>
    /// Suppresses code security whenever possible.
    /// </summary>
    [PublicAPI]
    SuppressSecurity = 1 << 5
}
