﻿//
//  InternalNullableAccessor.cs
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
using AdvancedDLSupport.Reflection.InternalLayout;
using JetBrains.Annotations;

namespace AdvancedDLSupport;

/// <summary>
/// Helper class for accessing the internal values of <see cref="Nullable{T}"/> instances.
/// </summary>
internal static class InternalNullableAccessor
{
    /// <summary>
    /// Accesses the underlying value of a <see cref="Nullable{T}"/> instance, referred to by the given pointer.
    /// </summary>
    /// <param name="nullablePtr">A pointer to a pinned nullable.</param>
    /// <typeparam name="T">The type of underlying value to access.</typeparam>
    /// <returns>The underlying value, passed by reference.</returns>
    [Pure]
    public static unsafe ref T AccessUnderlyingValue<T>(byte* nullablePtr) where T : struct
    {
        // HACK: Working around weird memory layout in .NET Core vs Mono/FX
        var offset = NullableTLayoutScanner<T>.PayloadOffset;
        nullablePtr += offset;

        return ref Unsafe.AsRef<T>(nullablePtr);
    }
}
