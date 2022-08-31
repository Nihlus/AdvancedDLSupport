//
//  NativeSymbolAttribute.cs
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
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace AdvancedDLSupport;

/// <summary>
/// Holds metadata for native functions.
/// </summary>
[PublicAPI, AttributeUsage(AttributeTargets.Method | AttributeTargets.Property)]
public sealed class NativeSymbolAttribute : Attribute
{
    /// <summary>
    /// Gets or sets the name of the function's entrypoint.
    /// </summary>
    [PublicAPI]
    public string Entrypoint { get; set; }

    /// <summary>
    /// Gets or sets the function's calling convention.
    /// </summary>
    [PublicAPI]
    public CallingConvention CallingConvention { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="NativeSymbolAttribute"/> class.
    /// </summary>
    /// <param name="entrypoint">The name of the function's entry point.</param>
    [PublicAPI]
    public NativeSymbolAttribute([CallerMemberName] string entrypoint = "")
    {
        CallingConvention = CallingConvention.Cdecl;
        Entrypoint = entrypoint;
    }
}
