//
//  StdCallEntrypointMangler.cs
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
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using AdvancedDLSupport.Reflection;
using JetBrains.Annotations;

namespace AdvancedDLSupport
{
    /// <summary>
    /// Mangles C-style functions decorated with the MSVC __stdcall attribute.
    /// </summary>
    internal class StdCallEntrypointMangler : IEntrypointMangler
    {
        /// <inheritdoc />
        public string Mangle<T>(T member) where T : IIntrospectiveMember
        {
            if (!(member is IntrospectiveMethodInfo method))
            {
                throw new NotSupportedException("The given member cannot be mangled by this mangler.");
            }

            var argumentListSize = method.ParameterTypes.Sum(CalculateArgumentSize);
            return $"_{method.GetTransformedNativeEntrypoint()}@{argumentListSize}";
        }

        /// <summary>
        /// Calculates the native size of the given argument.
        /// </summary>
        /// <param name="argumentType">The argument type.</param>
        /// <returns>The native size of the argument.</returns>
        private int CalculateArgumentSize([NotNull] Type argumentType)
        {
            if (argumentType == typeof(Delegate) || argumentType == typeof(MulticastDelegate))
            {
                return IntPtr.Size;
            }

            if (argumentType.IsEnum)
            {
                return Marshal.SizeOf(argumentType.GetEnumUnderlyingType());
            }

            if (argumentType.IsByRef)
            {
                return IntPtr.Size;
            }

            return Marshal.SizeOf(argumentType);
        }

        /// <inheritdoc />
        public string Demangle(string mangledEntrypoint)
        {
            return new string(mangledEntrypoint.Skip(1).TakeWhile(c => c != '@').ToArray());
        }

        /// <inheritdoc />
        public bool IsManglerApplicable(MemberInfo member)
        {
            if (!(member is IntrospectiveMethodInfo method))
            {
                return false;
            }

            var isApplicable =
                method.GetNativeCallingConvention() == CallingConvention.StdCall &&
                IntPtr.Size == 4 && // 32-bit system
                RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

            return isApplicable;
        }
    }
}
