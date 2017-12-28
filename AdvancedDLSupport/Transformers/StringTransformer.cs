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
