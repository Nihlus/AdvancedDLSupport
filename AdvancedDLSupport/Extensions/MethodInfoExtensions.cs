using System.Linq;
using System.Reflection;
using JetBrains.Annotations;

namespace AdvancedDLSupport.Extensions
{
    /// <summary>
    /// Extension methods for the <see cref="MethodInfo"/> class.
    /// </summary>
    internal static class MethodInfoExtensions
    {
        /// <summary>
        /// Determines whether or not the given method is complex,
        /// </summary>
        /// <param name="this">The method to check.</param>
        /// <returns>true if the method is complex; otherwise, false.</returns>
        [PublicAPI, Pure]
        public static bool IsComplexMethod([NotNull] this MethodInfo @this)
        {
            return HasComplexParameters(@this) || HasComplexReturnValue(@this);
        }

        /// <summary>
        /// Determines whether or not the method has complex parameters.
        /// </summary>
        /// <param name="this">The method.</param>
        /// <returns>true if the method has complex parameters; otherwise, false.</returns>
        [PublicAPI, Pure]
        public static bool HasComplexParameters([NotNull] this MethodBase @this)
        {
            var parameters = @this.GetParameters();
            return parameters.Any
            (
                p =>
                    p.ParameterType.IsComplexType()
            );
        }

        /// <summary>
        /// Determines whether or not the given method has a complex return type.
        /// </summary>
        /// <param name="this">The method.</param>
        /// <returns>true if the method has a complex return value; otherwise, false.</returns>
        [PublicAPI, Pure]
        public static bool HasComplexReturnValue([NotNull] this MethodInfo @this)
        {
            return @this.ReturnType.IsComplexType();
        }
    }
}
