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
        [Pure, PublicAPI]
        public static bool HasFlagFast<TEnum>(this TEnum value, TEnum flag)
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
        [Pure, PublicAPI]
        public static bool HasFlagsFast<TEnum>(this TEnum value, params TEnum[] flags)
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
        [Pure, PublicAPI]
        public static bool HasAll<TEnum>(this TEnum value)
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
        public static IEnumerable<TEnum> GetFlags<TEnum>(this TEnum input)
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

            foreach (TEnum value in Enum.GetValues(input.GetType()))
            {
                if (input.HasFlagFast(value))
                {
                    yield return value;
                }
            }
        }
    }
}
