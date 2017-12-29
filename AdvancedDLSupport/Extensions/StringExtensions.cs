//
//  StringExtensions.cs
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
using System.IO;
using System.Linq;
using JetBrains.Annotations;

namespace AdvancedDLSupport.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="string"/>.
    /// </summary>
    internal static class StringExtensions
    {
        /// <summary>
        /// Determines whether or not the given string is a valid path. This does not neccesarily indicate that
        /// the path exists.
        /// </summary>
        /// <param name="this">The string to inspect.</param>
        /// <returns>true if the string is a valid path; otherwise, false.</returns>
        [ContractAnnotation("this:null => false")]
        public static bool IsValidPath([CanBeNull] this string @this)
        {
            if (@this.IsNullOrWhiteSpace())
            {
                return false;
            }

            if (@this.Any(c => Path.GetInvalidPathChars().Contains(c)))
            {
                return false;
            }

            var parentDirectory = Path.GetDirectoryName(@this);
            if (parentDirectory.IsNullOrWhiteSpace())
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Determines whether or not a string is null or consists entirely of whitespace characters.
        /// </summary>
        /// <param name="this">The string to check.</param>
        /// <returns>true if the string is null or whitespace; otherwise, false.</returns>
        [Pure]
        [ContractAnnotation("this:null => true")]
        public static bool IsNullOrWhiteSpace([CanBeNull] this string @this)
        {
            return string.IsNullOrWhiteSpace(@this);
        }

        /// <summary>
        /// Determines whether or not a string is null or has no characters.
        /// </summary>
        /// <param name="this">The string to check.</param>
        /// <returns>true if the string is null or empty; otherwise, false.</returns>
        [Pure]
        [ContractAnnotation("this:null => true")]
        public static bool IsNullOrEmpty([CanBeNull] this string @this)
        {
            return string.IsNullOrEmpty(@this);
        }

        /// <summary>
        /// Determines whether or not a string contains another string using the given string comparer.
        /// </summary>
        /// <param name="this">The string to search.</param>
        /// <param name="search">The string to search for.</param>
        /// <param name="comparer">The string comparer to use.</param>
        /// <returns>true if the string contains the other string; otherwise, false.</returns>
        [Pure]
        [ContractAnnotation("this:null => false; search:null => false")]
        public static bool Contains([CanBeNull] this string @this, [CanBeNull] string search, StringComparison comparer)
        {
            return @this != null && search != null && @this.IndexOf(search, comparer) >= 0;
        }
    }
}
