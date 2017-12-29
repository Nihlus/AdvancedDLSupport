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
            return new CustomAttributeBuilder
            (
                @this.Constructor,
                @this.ConstructorArguments.Select(a => a.Value).ToArray()
            );
        }
    }
}
