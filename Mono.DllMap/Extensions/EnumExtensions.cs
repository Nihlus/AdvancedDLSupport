//
//  EnumExtensions.cs
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
using JetBrains.Annotations;

namespace Mono.DllMap.Extensions
{
    /// <summary>
    /// Extension methods for enums.
    /// </summary>
    [PublicAPI]
    public static class EnumExtensions
    {
        /// <summary>
        /// Checks if a flag is set on the given flag enum.
        /// </summary>
        /// <param name="value">The value to check against.</param>
        /// <param name="flag">The flag to check for.</param>
        /// <typeparam name="TEnum">The enum type.</typeparam>
        /// <returns>true if the value has the flag; otherwise, false.</returns>
        /// <exception cref="ArgumentException">
        /// Thrown if the type parameter is not an enum decorated with a flag attribute.
        /// </exception>
        [PublicAPI, Pure]
        public static bool HasFlagFast<TEnum>(this TEnum value, TEnum flag)
            where TEnum : struct, IComparable, IFormattable, IConvertible
        {
            ThrowIfEnumIsNotEnumOrNotFlags<TEnum>();

            return ((dynamic)value & (dynamic)flag) != 0;
        }

        /// <summary>
        /// Checks if all given flags are set on the given flag enum.
        /// </summary>
        /// <param name="value">The value to check against.</param>
        /// <param name="flags">The flags to check for.</param>
        /// <typeparam name="TEnum">The enum type.</typeparam>
        /// <returns>true if the value has all of the flags; otherwise, false.</returns>
        /// <exception cref="ArgumentException">
        /// Thrown if the type parameter is not an enum decorated with a flag attribute.
        /// </exception>
        [PublicAPI, Pure]
        public static bool HasFlagsFast<TEnum>(this TEnum value, [NotNull] params TEnum[] flags)
            where TEnum : struct, IComparable, IFormattable, IConvertible
        {
            ThrowIfEnumIsNotEnumOrNotFlags<TEnum>();

            return flags.All(f => value.HasFlagFast(f));
        }

        /// <summary>
        /// Determines whether or not the flag value is a compound value consisting of all available flags.
        /// </summary>
        /// <param name="value">The value to check.</param>
        /// <typeparam name="TEnum">The enum type.</typeparam>
        /// <returns>true if it is all flag values; otherwise, false.</returns>
        /// <exception cref="ArgumentException">
        /// Thrown if the type parameter is not an enum decorated with a flag attribute.
        /// </exception>
        [PublicAPI, Pure]
        public static bool HasAll<TEnum>(this TEnum value)
            where TEnum : struct, IComparable, IFormattable, IConvertible
        {
            ThrowIfEnumIsNotEnumOrNotFlags<TEnum>();

            return Enum.GetValues(typeof(TEnum)).Cast<TEnum>().All(v => value.HasFlagFast(v));
        }

        /// <summary>
        /// Gets all set flag values from the given enum value.
        /// </summary>
        /// <param name="input">The enum.</param>
        /// <typeparam name="TEnum">The enum type.</typeparam>
        /// <returns>A list of set flags.</returns>
        /// <exception cref="ArgumentException">
        /// Thrown if the type parameter is not an enum decorated with a flag attribute.
        /// </exception>
        [PublicAPI, Pure]
        public static IEnumerable<TEnum> GetFlags<TEnum>(this TEnum input)
            where TEnum : struct, IComparable, IFormattable, IConvertible
        {
            ThrowIfEnumIsNotEnumOrNotFlags<TEnum>();

            foreach (TEnum value in Enum.GetValues(input.GetType()))
            {
                if (input.HasFlagFast(value))
                {
                    yield return value;
                }
            }
        }

        /// <summary>
        /// Throws an <see cref="ArgumentException"/> if the provided generic type is not an enum decorated with a
        /// <see cref="FlagsAttribute"/>.
        /// </summary>
        /// <typeparam name="TEnum">The type to check.</typeparam>
        /// <exception cref="ArgumentException">Thrown if the generic type is not a flag enum.</exception>
        private static void ThrowIfEnumIsNotEnumOrNotFlags<TEnum>()
            where TEnum : struct, IComparable, IFormattable, IConvertible
        {
            if (!typeof(TEnum).IsEnum)
            {
                throw new ArgumentException("The provided type was not an enum.", nameof(TEnum));
            }

            if (typeof(TEnum).GetCustomAttribute<FlagsAttribute>() is null)
            {
                throw new ArgumentException("The provided enum type was not a flag enum.", nameof(TEnum));
            }
        }
    }
}
