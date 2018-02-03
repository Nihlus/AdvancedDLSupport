//
//  ImplementationOptions.cs
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

namespace AdvancedDLSupport
{
    /// <summary>
    /// Holds generated implementation flag options.
    /// </summary>
    [Flags]
    public enum ImplementationOptions
    {
        /// <summary>
        /// Generate the bindings with lazy loaded symbol resolution.
        /// </summary>
        UseLazyBinding = 1 << 0,

        /// <summary>
        /// Generate disposal checks for all binder methods.
        /// </summary>
        GenerateDisposalChecks = 1 << 1,

        /// <summary>
        /// Enable Mono dllmap support for library scanning.
        /// </summary>
        EnableDllMapSupport = 1 << 2
    }
}
