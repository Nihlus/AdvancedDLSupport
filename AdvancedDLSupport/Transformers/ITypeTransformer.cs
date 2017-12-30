using System;
using JetBrains.Annotations;

#pragma warning disable SA1402

namespace AdvancedDLSupport
{
    /// <summary>
    /// Opaque interface, used for differentiating type transformers from arbitrary objects.
    /// </summary>
    [PublicAPI]
    public interface ITypeTransformer
    {
        /// <summary>
        /// Lowers the type information of the more complex type to the less complex type.
        /// </summary>
        /// <returns>The lowered type.</returns>
        [PublicAPI]
        Type LowerType();

        /// <summary>
        /// Raises the type information of the less complex type to the more complex type.
        /// </summary>
        /// <returns>The lowered type.</returns>
        [PublicAPI]
        Type RaiseType();
    }

    /// <summary>
    /// Represents a transformer, which can lower or raise values between two types, altering their complexity without
    /// data loss.
    /// </summary>
    /// <typeparam name="T1">The type considered more complex than the other.</typeparam>
    /// <typeparam name="T2">The type consideres less complex than the other.</typeparam>
    [PublicAPI]
    public interface ITypeTransformer<T1, T2> : ITypeTransformer
    {
        /// <summary>
        /// Lowers a value of type <typeparamref name="T1"/> to a value of type <typeparamref name="T2"/>.
        /// </summary>
        /// <param name="value">The value to lower.</param>
        /// <returns>A lowered value.</returns>
        [PublicAPI, CanBeNull]
        T2 LowerValue([CanBeNull] T1 value);

        /// <summary>
        /// Raises a value of type <typeparamref name="T2"/> to a value of type <typeparamref name="T1"/>.
        /// </summary>
        /// <param name="value">The value to raise.</param>
        /// <returns>A raised value.</returns>
        [PublicAPI, CanBeNull]
        T1 RaiseValue([CanBeNull] T2 value);
    }
}
