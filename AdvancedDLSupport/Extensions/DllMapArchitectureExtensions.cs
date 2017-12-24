﻿using AdvancedDLSupport.DllMap.Mono;
using JetBrains.Annotations;

namespace AdvancedDLSupport.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="DllMapArchitecture"/>.
    /// </summary>
    internal static class DllMapArchitectureExtensions
    {
        /// <summary>
        /// Checks if a flag is set on the given flag enum.
        /// </summary>
        /// <param name="value">The value to check against.</param>
        /// <param name="flag">The flag to check for.</param>
        /// <returns>true if the value has the flag; otherwise, false.</returns>
        [Pure]
        public static bool HasFlagFast(this DllMapArchitecture value, DllMapArchitecture flag)
        {
            return (value & flag) != 0;
        }
    }
}
