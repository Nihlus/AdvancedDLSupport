//
//  DllMapArchitecture.cs
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

// ReSharper disable MultipleSpaces
// ReSharper disable InconsistentNaming
#pragma warning disable CS1591, SA1600, SA1602, SA1025, SA1300

namespace Mono.DllMap
{
    [PublicAPI, Flags]
    public enum DllMapArchitecture
    {
        x86    = 1 << 0,
        x86_64 = 1 << 1,
        SPARC  = 1 << 2,
        PPC    = 1 << 3,
        S390   = 1 << 4,
        S390X  = 1 << 5,
        ARM    = 1 << 6,
        ARMV8  = 1 << 7,
        MIPS   = 1 << 8,
        Alpha  = 1 << 9,
        HPPA   = 1 << 10,
        IA64   = 1 << 11
    }
}
