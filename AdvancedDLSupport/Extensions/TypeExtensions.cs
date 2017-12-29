using System;
using JetBrains.Annotations;

namespace AdvancedDLSupport.Extensions
{
    /// <summary>
    /// Extension methods for the <see cref="Type"/> class.
    /// </summary>
    internal static class TypeExtensions
    {
        /// <summary>
        /// Determines whether or not the given type is a complex type.
        /// </summary>
        /// <param name="this">The type.</param>
        /// <returns>true if the type is complex; otherwise, false.</returns>
        [PublicAPI, Pure]
        public static bool IsComplexType([NotNull] this Type @this)
        {
            return
                @this == typeof(string) ||
                (@this.IsGenericType && @this.GetGenericTypeDefinition() == typeof(Nullable<>));
        }
    }
}
