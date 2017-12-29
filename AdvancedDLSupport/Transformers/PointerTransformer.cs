using System;
using System.Reflection;
using JetBrains.Annotations;

namespace AdvancedDLSupport
{
    /// <summary>
    /// Lowers or raises types to or from <see cref="IntPtr"/>s.
    /// </summary>
    /// <typeparam name="T">The complex type that can be changed into a pointer.</typeparam>
    [PublicAPI]
    public abstract class PointerTransformer<T> : ITypeTransformer<T, IntPtr>
    {
        /// <inheritdoc />
        public abstract IntPtr LowerValue(T value, ParameterInfo parameter);

        /// <inheritdoc />
        public abstract T RaiseValue(IntPtr value, ParameterInfo parameter);

        /// <inheritdoc />
        public Type LowerType()
        {
            return typeof(IntPtr);
        }

        /// <inheritdoc />
        public Type RaiseType()
        {
            return typeof(T);
        }
    }
}
