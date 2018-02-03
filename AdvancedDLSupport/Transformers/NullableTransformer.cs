//
//  NullableTransformer.cs
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

namespace AdvancedDLSupport
{
    /// <summary>
    /// Raises or lowers nullable value types to pointers.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    [PublicAPI]
    public class NullableTransformer<T> : PointerTransformer<T?> where T : struct
    {
        /// <inheritdoc />
        public override IntPtr LowerValue(T? value)
        {
            if (!value.HasValue)
            {
                return IntPtr.Zero;
            }

            // TODO: Add a way to free this memory after use
            var ptr = Marshal.AllocHGlobal(Marshal.SizeOf<T>());
            Marshal.StructureToPtr(value.Value, ptr, false);

            return ptr;
        }

        /// <inheritdoc />
        public override T? RaiseValue(IntPtr value)
        {
            if (value == IntPtr.Zero)
            {
                return null;
            }

            var val = Marshal.PtrToStructure<T>(value);
            return val;
        }
    }
}
