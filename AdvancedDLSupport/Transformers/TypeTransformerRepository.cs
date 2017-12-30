using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace AdvancedDLSupport
{
    /// <summary>
    /// Repository class for type transformers.
    /// </summary>
    [PublicAPI]
    public class TypeTransformerRepository
    {
        private readonly Dictionary<Type, ITypeTransformer> _typeTransformers = new Dictionary<Type, ITypeTransformer>();

        /// <summary>
        /// Adds the given type transformer to the repository, making it available to complex generators.
        /// </summary>
        /// <param name="type">The type to map the transformer to.</param>
        /// <param name="transformer">The transformer to add.</param>
        /// <typeparam name="T1">The more complex type.</typeparam>
        /// <typeparam name="T2">The less complex type.</typeparam>
        /// <returns>The repository, with the transformer added.</returns>
        [PublicAPI, NotNull]
        public TypeTransformerRepository WithTypeTransformer<T1, T2>([NotNull] Type type, [NotNull] ITypeTransformer<T1, T2> transformer)
        {
            if (!_typeTransformers.ContainsKey(type))
            {
                _typeTransformers.Add(type, transformer);
            }

            return this;
        }

        /// <summary>
        /// Gets the transformer for the given complex type.
        /// </summary>
        /// <param name="type">The complex type.</param>
        /// <returns>The type transformer for the complex value.</returns>
        /// <exception cref="NotSupportedException">Thrown if no compatible transformer can be found.</exception>
        [PublicAPI, NotNull]
        public ITypeTransformer GetComplexTransformer([NotNull] Type type)
        {
            if (type == typeof(string))
            {
                return GetStringTransformer();
            }

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                var innerType = type.GetGenericArguments().First();
                var openNullableGetter = typeof(TypeTransformerRepository).GetMethod(nameof(GetNullableTransformer));
                var closedNullableGetter = openNullableGetter.MakeGenericMethod(innerType);

                return (ITypeTransformer)closedNullableGetter.Invoke(this, null);
            }

            if (_typeTransformers.ContainsKey(type))
            {
                return _typeTransformers[type];
            }

            throw new NotSupportedException("The given type doesn't have a compatible type transformer.");
        }

        /// <summary>
        /// Gets a type transformer which can transform strings to pointers and vice versa.
        /// </summary>
        /// <returns>A string transformer.</returns>
        [PublicAPI, NotNull]
        public StringTransformer GetStringTransformer()
        {
            if (_typeTransformers.ContainsKey(typeof(string)))
            {
                return (StringTransformer)_typeTransformers[typeof(string)];
            }

            var transformer = new StringTransformer();
            _typeTransformers.Add(typeof(string), transformer);

            return transformer;
        }

        /// <summary>
        /// Gets a type transformer which can transform nullable value types to pointers and vice versa.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <returns>A nullable transformer.</returns>
        [PublicAPI, NotNull]
        public NullableTransformer<T> GetNullableTransformer<T>() where T : struct
        {
            if (_typeTransformers.ContainsKey(typeof(T?)))
            {
                return (NullableTransformer<T>)_typeTransformers[typeof(T?)];
            }

            var transformer = new NullableTransformer<T>();
            _typeTransformers.Add(typeof(T?), transformer);

            return transformer;
        }
    }
}
