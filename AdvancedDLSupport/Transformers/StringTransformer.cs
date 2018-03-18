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
using System.Reflection;
using System.Runtime.InteropServices;
using static System.Runtime.InteropServices.UnmanagedType;

#pragma warning disable SA1513

namespace AdvancedDLSupport
{
    /// <summary>
    /// Raises or lowers strings.
    /// </summary>
    internal class StringTransformer : PointerTransformer<string>
    {
        private readonly IReadOnlyList<UnmanagedType> _supportedTypes = new[]
        {
            BStr,
            LPStr,
            LPTStr,
            LPWStr
        };

        /// <inheritdoc />
        public override IntPtr LowerValue(string value, ParameterInfo parameter)
        {
            if (value is null)
            {
                return IntPtr.Zero;
            }

            var unmanagedType = GetCustomUnmanagedTypeOrDefault(parameter);

            IntPtr ptr;
            switch (unmanagedType)
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
                    if (RuntimeInformation.FrameworkDescription.Contains("Mono"))
                    {
                        // Mono uses ANSI for Auto, but ANSI is no longer a supported charset. Use Unicode.
                        ptr = Marshal.StringToHGlobalUni(value);
                    }
                    else
                    {
                        // Use automatic selection
                        ptr = Marshal.StringToHGlobalAuto(value);
                    }

                    break;
                }
                case LPWStr:
                {
                    ptr = Marshal.StringToHGlobalUni(value);
                    break;
                }
                default:
                {
                    throw new ArgumentOutOfRangeException
                    (
                        nameof(unmanagedType),
                        "The unmanaged type wasn't set to a recognized string type."
                    );
                }
            }

            return ptr;
        }

        private UnmanagedType GetCustomUnmanagedTypeOrDefault(ParameterInfo parameter)
        {
            var unmanagedType = LPStr;
            var marshalAsAttribute = parameter.GetCustomAttribute<MarshalAsAttribute>();
            if (!(marshalAsAttribute is null))
            {
                var customUnmanagedType = marshalAsAttribute.Value;
                if (_supportedTypes.Contains(customUnmanagedType))
                {
                    unmanagedType = customUnmanagedType;
                }
            }

            return unmanagedType;
        }

        /// <inheritdoc />
        public override string RaiseValue(IntPtr value, ParameterInfo parameter)
        {
            if (value == IntPtr.Zero)
            {
                return null;
            }

            var unmanagedType = GetCustomUnmanagedTypeOrDefault(parameter);

            string val;
            switch (unmanagedType)
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
                    if (RuntimeInformation.FrameworkDescription.Contains("Mono"))
                    {
                        // Mono uses ANSI for Auto, but ANSI is no longer a supported charset. Use Unicode.
                        val = Marshal.PtrToStringUni(value);
                    }
                    else
                    {
                        // Use automatic selection
                        val = Marshal.PtrToStringAuto(value);
                    }

                    break;
                }
                case LPWStr:
                {
                    val = Marshal.PtrToStringUni(value);
                    break;
                }
                default:
                {
                    throw new ArgumentOutOfRangeException
                    (
                        nameof(unmanagedType),
                        "The unmanaged type wasn't set to a recognized string type."
                    );
                }
            }

            return val;
        }
    }
}
