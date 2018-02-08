//
//  InternalNullableAccessor.cs
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
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace AdvancedDLSupport
{
    /// <summary>
    /// Helper class for accessing the internal values of <see cref="Nullable{T}"/> instances.
    /// </summary>
    public static class InternalNullableAccessor
    {
        /// <summary>
        /// Accesses the underlying value of a <see cref="Nullable{T}"/> instance, referred to by the given pointer.
        /// </summary>
        /// <param name="nullablePtr">A pointer to a pinned nullable.</param>
        /// <typeparam name="T">The type of underlying value to access.</typeparam>
        /// <returns>The underlying value, passed by reference.</returns>
        public static unsafe ref T AccessUnderlyingValue<T>(int* nullablePtr) where T : struct
        {
            // HACK: Working around weird memory layout in .NET Core vs Mono/FX
            var desc = RuntimeInformation.FrameworkDescription;
            if (desc.Contains(".NET Core"))
            {
                nullablePtr += Unsafe.SizeOf<bool>();
            }

            return ref Unsafe.AsRef<T>(nullablePtr);
        }
    }
}
