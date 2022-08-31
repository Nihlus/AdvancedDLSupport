//
//  CustomAttributeDataExtensions.cs
//
//  Author:
//       Jarl Gullberg <jarl.gullberg@gmail.com>
//
//  Copyright (c) Jarl Gullberg
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
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using JetBrains.Annotations;

namespace AdvancedDLSupport.Extensions
{
    /// <summary>
    /// Extension methods for the <see cref="CustomAttributeData"/> class.
    /// </summary>
    internal static class CustomAttributeDataExtensions
    {
        /// <summary>
        /// Gets an attribute builder for the given attribute data instance.
        /// </summary>
        /// <param name="this">The attribute data to create a builder for.</param>
        /// <returns>An attribute builder.</returns>
        [NotNull, Pure]
        public static CustomAttributeBuilder GetAttributeBuilder([NotNull] this CustomAttributeData @this)
        {
            var namedFields = @this.NamedArguments?.Where(a => a.IsField).ToList() ?? new List<CustomAttributeNamedArgument>();
            var namedProperties = @this.NamedArguments?.Where(a => a.MemberInfo is PropertyInfo).ToList() ?? new List<CustomAttributeNamedArgument>();

            return new CustomAttributeBuilder
            (
                @this.Constructor,
                @this.ConstructorArguments.Select(a => a.Value).ToArray(),
                namedProperties.Select(p => p.MemberInfo).Cast<PropertyInfo>().ToArray(),
                namedProperties.Select(p => p.TypedValue.Value).ToArray(),
                namedFields.Select(f => f.MemberInfo).Cast<FieldInfo>().ToArray(),
                namedFields.Select(f => f.TypedValue.Value).ToArray()
            );
        }

        /// <summary>
        /// Uses the attribute data to create an instance of the attribute.
        /// </summary>
        /// <param name="this">The attribute data.</param>
        /// <typeparam name="T">The encapsulated type of the attribute.</typeparam>
        /// <returns>An instance of the attribute as described by the attribute data.</returns>
        /// <exception cref="ArgumentException">Thrown if the attribute type and the generic type doesn't match.</exception>
        [NotNull]
        public static T ToInstance<T>([NotNull] this CustomAttributeData @this) where T : Attribute
        {
            if (typeof(T) != @this.AttributeType)
            {
                throw new ArgumentException($"Incorrect generic argument type. Use {@this.AttributeType.Name}.", nameof(@this));
            }

            var instance = @this.Constructor.Invoke(@this.ConstructorArguments.Select(a => a.Value).ToArray());

            var namedFields = @this.NamedArguments?.Where(a => a.IsField).ToList();
            foreach (var field in namedFields ?? new List<CustomAttributeNamedArgument>())
            {
                (field.MemberInfo as FieldInfo)?.SetValue(instance, field.TypedValue.Value);
            }

            var namedProperties = @this.NamedArguments?.Where(a => a.MemberInfo is PropertyInfo).ToList();
            foreach (var property in namedProperties ?? new List<CustomAttributeNamedArgument>())
            {
                (property.MemberInfo as PropertyInfo)?.SetValue(instance, property.TypedValue.Value);
            }

            return (T)instance;
        }
    }
}
