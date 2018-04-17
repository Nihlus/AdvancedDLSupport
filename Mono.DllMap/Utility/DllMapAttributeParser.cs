//
//  DllMapAttributeParser.cs
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
using JetBrains.Annotations;

namespace Mono.DllMap.Utility
{
    /// <summary>
    /// Parses DllMap attribute lists.
    /// </summary>
    [PublicAPI]
    public static class DllMapAttributeParser
    {
        /// <summary>
        /// Parses the given string as a DllMap attribute list.
        /// </summary>
        /// <param name="content">
        /// The attribute list. This is a comma-separated list of constrained values. The list can optionally be
        /// prefixed with '!' to invert its meaning.
        /// </param>
        /// <typeparam name="TEnum">
        /// The enum type to parse into. This enum must be an enum decorated with a flag attribute.
        /// </typeparam>
        /// <returns>A compound flag value.</returns>
        /// <exception cref="ArgumentException">
        /// Thrown if the type parameter is not an enum decorated with a flag attribute.
        /// </exception>
        [PublicAPI, Pure]
        public static TEnum Parse<TEnum>([CanBeNull] string content) where TEnum : struct
        {
            if (!typeof(TEnum).IsEnum)
            {
                throw new ArgumentException("The provided type was not an enum.", nameof(TEnum));
            }

            if (typeof(TEnum).GetCustomAttribute<FlagsAttribute>() is null)
            {
                throw new ArgumentException("The provided enum type was not a flag enum.", nameof(TEnum));
            }

            if (string.IsNullOrWhiteSpace(content))
            {
                return Enum
                    .GetValues(typeof(TEnum))
                    .Cast<TEnum>()
                    .Aggregate((a, b) => (dynamic)a | (dynamic)b);
            }

            bool isInverse = false;

            // ReSharper disable once PossibleNullReferenceException
            var parsingString = content.Replace('-', '_');
            if (parsingString.First() == '!')
            {
                parsingString = new string(parsingString.Skip(1).ToArray());
                isInverse = true;
            }

            var parts = parsingString.Split(',');
            var systems = parts.Select
                (
                    p =>
                    (
                        CouldParse: Enum.TryParse(p, true, out TEnum x),
                        Value: x
                    )
                )
                .Where(t => t.CouldParse)
                .Select(t => t.Value).Distinct();

            if (isInverse)
            {
                systems = Enum.GetValues(typeof(TEnum)).Cast<TEnum>().Except(systems);
            }

            return systems.Aggregate((a, b) => (dynamic)a | (dynamic)b);
        }
    }
}
