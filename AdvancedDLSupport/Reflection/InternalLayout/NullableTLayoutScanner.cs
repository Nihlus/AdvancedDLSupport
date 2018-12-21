//
//  NullableTLayoutScanner.cs
//
//  Copyright (c) 2018 Firwood Software
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

namespace AdvancedDLSupport.Reflection.InternalLayout
{
    /// <summary>
    /// Scans the internal memory layout of <see cref="Nullable{T}"/> structures to determine the offset of the data
    /// payload. The structure appears to shift depending on the size, layout, and packing of the structure itself, so
    /// the structure type is provided as a generic argument, and an offset is calculated on a type-by-type basis.
    ///
    /// For future information, and to the next guy: having a generic type argument on a static type results in one
    /// unique static type per type parameter combination. This means that a NullableTLayoutScanner{T1} is NOT the same
    /// type as a NullableTLayoutScanner{T2}, and have separate static class instances.
    /// </summary>
    /// <typeparam name="TStructure">The type of the structure to scan for.</typeparam>
    internal static class NullableTLayoutScanner<TStructure> where TStructure : struct
    {
        /// <summary>
        /// Holds the initial signature value which is used to identify the structure.
        /// </summary>
        private const byte InitialSignatureValue = 3;

        /// <summary>
        /// Holds the payload offset for the structure type provided as a generic argument.
        /// </summary>
        // ReSharper disable once StaticMemberInGenericType
        private static int? _payloadOffset;

        /// <summary>
        /// Gets the payload offset into a <see cref="Nullable{T}"/> structure.
        /// </summary>
        public static int PayloadOffset
        {
            get
            {
                if (_payloadOffset is null)
                {
                    _payloadOffset = FindPayloadOffset();
                }

                return _payloadOffset.Value;
            }
        }

        /// <summary>
        /// Creates a payload structure filled with predictable data.
        /// </summary>
        /// <returns>The payload.</returns>
        private static TStructure CreatePayload()
        {
            var payload = default(TStructure);

            var structureSize = Unsafe.SizeOf<TStructure>();
            unsafe
            {
                // Fill the structure with predictable (but junk) data
                var ptr = (byte*)Unsafe.AsPointer(ref payload);
                for (var i = 0; i < structureSize; ++i)
                {
                    *ptr = unchecked((byte)(InitialSignatureValue + i));
                    ++ptr;
                }
            }

            return payload;
        }

        /// <summary>
        /// Scans <see cref="Nullable{T}"/> for the payload offset, and returns it.
        /// </summary>
        /// <returns>The payload offset.</returns>
        /// <exception cref="IndexOutOfRangeException">Thrown if the resulting offset falls outside the T?.</exception>
        private static int FindPayloadOffset()
        {
            var payload = CreatePayload();

            TStructure? nullablePayload = payload;
            unsafe
            {
                var nullableSize = Unsafe.SizeOf<TStructure?>();
                var structureSize = Unsafe.SizeOf<TStructure>();
                var ptr = (byte*)Unsafe.AsPointer(ref nullablePayload);
                var offset = 0;

                // This algorithm is fairly simple, but still warrants an explanation. Since we cannot reasonably
                // predict the layout of a given T? at compile time due to variations between runtimes (and runtime
                // versions), we have to search for the structure ourselves within the bounds of a T?.
                //
                // This is done by creating a payload structure filled with a predictable sequence of values, and then
                // searching for the offset at which that sequence occurs within the T?. If we go out of bounds,
                // something is wrong with the algorithm, and we bail out.
                while (true)
                {
                    var value = *ptr;
                    if (value != InitialSignatureValue)
                    {
                        ptr++;
                        offset++;

                        if (offset >= nullableSize)
                        {
                            throw new IndexOutOfRangeException
                            (
                                "The payload could not be found within the bounds of the nullable structure."
                            );
                        }

                        continue;
                    }

                    var hasGoodSignature = true;
                    var scanPtr = ptr;
                    for (var i = 0; i < structureSize; ++i)
                    {
                        if (*scanPtr == unchecked((byte)(InitialSignatureValue + i)))
                        {
                            scanPtr++;
                            continue;
                        }

                        hasGoodSignature = false;
                        break;
                    }

                    if (hasGoodSignature)
                    {
                        return offset;
                    }
                }
            }
        }
    }
}
