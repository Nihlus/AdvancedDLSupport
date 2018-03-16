//
//  StdCallEntrypointMangler.cs
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
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using AdvancedDLSupport.Reflection;

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
            if (member is IntrospectiveMethodInfo method)
            {
                var argumentListSize = method.ParameterTypes.Sum(Marshal.SizeOf);
                return $"_{method.Name}@{argumentListSize}";
            }

            throw new NotSupportedException("The given member cannot be mangled by this mangler.");
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

            var metadataAttribute = method.GetCustomAttribute<NativeSymbolAttribute>()
                                    ?? new NativeSymbolAttribute(method.Name);

            var isApplicable =
                metadataAttribute.CallingConvention == CallingConvention.StdCall &&
                IntPtr.Size == 4 && // 32-bit system
                RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

            return isApplicable;
        }
    }
}
