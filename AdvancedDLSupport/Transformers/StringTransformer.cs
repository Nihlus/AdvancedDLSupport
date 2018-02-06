//
//  StringTransformer.cs
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
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using JetBrains.Annotations;
using static System.Runtime.InteropServices.UnmanagedType;

#pragma warning disable SA1513

namespace AdvancedDLSupport
{
    /// <summary>
    /// Raises or lowers strings.
    /// </summary>
    [PublicAPI]
    public class StringTransformer : PointerTransformer<string>
    {
        private readonly UnmanagedType _unmanagedType;

        private readonly IReadOnlyList<UnmanagedType> _supportedTypes = new[]
        {
            BStr,
            LPStr,
            LPTStr,
            LPWStr
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="StringTransformer"/> class.
        /// </summary>
        /// <param name="unmanagedType">The unmanaged type to marshal strings as.</param>
        /// <exception cref="ArgumentException">Thrown if the unmanaged type is not a string type.</exception>
        public StringTransformer(UnmanagedType unmanagedType = LPStr)
        {
            if (!_supportedTypes.Contains(unmanagedType))
            {
                throw new ArgumentException("The given unmanaged type was not a string type.", nameof(unmanagedType));
            }

            _unmanagedType = unmanagedType;
        }

        /// <inheritdoc />
        public override IntPtr LowerValue(string value)
        {
            if (value == null)
            {
                return IntPtr.Zero;
            }

            IntPtr ptr;
            switch (_unmanagedType)
            {
                case BStr:
                {
                    ptr = Marshal.StringToBSTR(value);
                    break;
                }
                case LPStr:
                {
                    ptr = Marshal.StringToHGlobalAnsi(value);
                    break;
                }
                case LPTStr:
                {
                    ptr = Marshal.StringToHGlobalAuto(value);
                    break;
                }
                case LPWStr:
                {
                    ptr = Marshal.StringToHGlobalUni(value);
                    break;
                }
                default:
                {
                    throw new ArgumentOutOfRangeException();
                }
            }

            return ptr;
        }

        /// <inheritdoc />
        public override string RaiseValue(IntPtr value)
        {
            if (value == IntPtr.Zero)
            {
                return null;
            }

            string val;
            switch (_unmanagedType)
            {
                case BStr:
                {
                    val = Marshal.PtrToStringBSTR(value);
                    break;
                }
                case LPStr:
                {
                    val = Marshal.PtrToStringAnsi(value);
                    break;
                }
                case LPTStr:
                {
                    val = Marshal.PtrToStringAuto(value);
                    break;
                }
                case LPWStr:
                {
                    val = Marshal.PtrToStringUni(value);
                    break;
                }
                default:
                {
                    throw new ArgumentOutOfRangeException();
                }
            }

            return val;
        }
    }
}
