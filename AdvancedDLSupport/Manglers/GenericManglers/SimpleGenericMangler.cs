//
//  SimpleGenericMangler.cs
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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using AdvancedDLSupport.Reflection;

namespace AdvancedDLSupport
{
    /// <summary>
    /// Performs simple heuristic mangling of generic methods.
    /// </summary>
    public class SimpleGenericMangler : IEntrypointMangler
    {
        private static readonly IReadOnlyDictionary<Type, string> TypeSuffixes = new Dictionary<Type, string>
        {
            { typeof(float), "f" },
            { typeof(double), "d" },
            { typeof(sbyte), "sb" },
            { typeof(byte), "b" },
            { typeof(short), "s" },
            { typeof(int), "i" },
            { typeof(long), "l" },
        };

        private static readonly IReadOnlyList<Type> UnsignedTypes = new[]
        {
            typeof(ushort),
            typeof(uint),
            typeof(ulong),
        };

        /// <inheritdoc/>
        public bool IsManglerApplicable(MemberInfo member)
        {
            if (!(member is MethodInfo methodInfo))
            {
                return false;
            }

            return methodInfo.IsGenericMethod;
        }

        /// <inheritdoc/>
        public string Mangle<T>(T member) where T : IIntrospectiveMember
        {
            var methodInfo = (member as IntrospectiveMethodInfo)?.GetWrappedMember();

            if (methodInfo is null)
            {
                throw new InvalidOperationException("Failed to unwrap the member into a method info.");
            }

            var arguments = methodInfo.GetGenericArguments();

            var name = member.GetCustomAttribute<NativeSymbolAttribute>()?.Entrypoint ?? member.Name;

            // Take the name, and in order, append the type suffix
            var builder = new StringBuilder(name);

            foreach (var argumentType in arguments)
            {
                if (TypeSuffixes.ContainsKey(argumentType))
                {
                    builder.Append(TypeSuffixes[argumentType]);
                    if (UnsignedTypes.Contains(argumentType))
                    {
                        builder.Append("u");
                    }
                }
                else if (argumentType.IsValueType && !argumentType.IsPrimitive)
                {
                    // It's probably a struct
                    builder.Append("st");
                }

                // Check if the type is a reflike type (pointer, out, ref, etc)
                var isRefLike = argumentType.IsByRef || argumentType.IsPointer;
                if (isRefLike)
                {
                    builder.Append("v");
                }
            }

            return builder.ToString();
        }

        /// <inheritdoc/>
        public string Demangle(string mangledEntrypoint)
        {
            throw new NotSupportedException();
        }
    }
}
