using System;
using System.IO;
using System.Linq;
using JetBrains.Annotations;

namespace AdvancedDLSupport.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="string"/>.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Determines whether or not the given string is a valid path. This does not neccesarily indicate that
        /// the path exists.
        /// </summary>
        /// <param name="source">The string to inspect.</param>
        /// <returns>true if the string is a valid path; otherwise, false.</returns>
        public static bool IsValidPath([CanBeNull] this string source)
        {
            if (source.IsNullOrWhiteSpace())
            {
                return false;
            }

            if (source.Any(c => Path.GetInvalidPathChars().Contains(c)))
            {
                return false;
            }

            var parentDirectory = Path.GetDirectoryName(source);
            if (parentDirectory.IsNullOrWhiteSpace())
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Determines whether or not a string is null or consists entirely of whitespace characters.
        /// </summary>
        /// <param name="source">The string to check.</param>
        /// <returns>true if the string is null or whitespace; otherwise, false.</returns>
        [Pure]
        [ContractAnnotation("source:null => true")]
        public static bool IsNullOrWhiteSpace([CanBeNull] this string source)
        {
            return string.IsNullOrWhiteSpace(source);
        }

        /// <summary>
        /// Determines whether or not a string is null or has no characters.
        /// </summary>
        /// <param name="source">The string to check.</param>
        /// <returns>true if the string is null or empty; otherwise, false.</returns>
        [Pure]
        [ContractAnnotation("source:null => true")]
        public static bool IsNullOrEmpty([CanBeNull] this string source)
        {
            return string.IsNullOrEmpty(source);
        }

        /// <summary>
        /// Determines whether or not a string contains another string using the given string comparer.
        /// </summary>
        /// <param name="this">The string to search.</param>
        /// <param name="search">The string to search for.</param>
        /// <param name="comparer">The string comparer to use.</param>
        /// <returns>true if the string contains the other string; otherwise, false.</returns>
        [Pure]
        public static bool Contains([CanBeNull] this string @this, [CanBeNull] string search, StringComparison comparer)
        {
            return @this != null && search != null && @this.IndexOf(search, comparer) >= 0;
        }
    }
}
