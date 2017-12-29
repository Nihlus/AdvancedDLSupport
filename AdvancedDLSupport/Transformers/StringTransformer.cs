using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
            if (value == null)
            {
                return IntPtr.Zero;
            }

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
        public override string RaiseValue(IntPtr value, ParameterInfo parameter)
        {
            if (value == IntPtr.Zero)
            {
                return null;
            }

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
