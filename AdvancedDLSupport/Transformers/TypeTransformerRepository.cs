//
//  TypeTransformerRepository.cs
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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AdvancedDLSupport.Extensions;
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
        /// Initializes a new instance of the <see cref="TypeTransformerRepository"/> class.
        /// </summary>
        public TypeTransformerRepository()
        {
            // Add default transformers
            _typeTransformers.Add(typeof(string), new StringTransformer());
        }

        /// <summary>
        /// Adds the given type transformer to the repository, making it available to complex generators.
        /// </summary>
        /// <param name="type">The type to map the transformer to.</param>
        /// <param name="transformer">The transformer to add.</param>
        /// <typeparam name="T1">The more complex type.</typeparam>
        /// <typeparam name="T2">The less complex type.</typeparam>
        /// <returns>The repository, with the transformer added.</returns>
        [PublicAPI, NotNull]
        public TypeTransformerRepository WithTypeTransformer<T1, T2>
        (
            [NotNull] Type type,
            [NotNull] ITypeTransformer<T1, T2> transformer
        )
        {
            if (!_typeTransformers.ContainsKey(type))
            {
                _typeTransformers.Add(type, transformer);
            }

            return this;
        }

        /// <summary>
        /// Determines whether or not the repository contains an applicable type transformer.
        /// </summary>
        /// <param name="complexType">The complex type to transform.</param>
        /// <param name="options">The implementation options to use.</param>
        /// <returns>true if the repository contains an applicable transformer; otherwise, false.</returns>
        [PublicAPI]
        public bool HasApplicableTransformer([NotNull] Type complexType, ImplementationOptions options)
        {
            // HACK: hard-coded option for nullable transformer
            if (complexType.IsNonRefNullable())
            {
                return true;
            }

            return _typeTransformers.Values.Any(t => t.IsApplicable(complexType, options));
        }

        /// <summary>
        /// Gets the transformer for the given complex type.
        /// </summary>
        /// <param name="type">The complex type.</param>
        /// <returns>The type transformer for the complex value.</returns>
        /// <exception cref="NotSupportedException">Thrown if no compatible transformer can be found.</exception>
        [PublicAPI, NotNull]
        public ITypeTransformer GetTypeTransformer([NotNull] Type type)
        {
            if (_typeTransformers.ContainsKey(type))
            {
                return _typeTransformers[type];
            }

            // HACK: hard-coded option for nullable transformer
            if (type.IsNonRefNullable())
            {
                var innerType = type.GetGenericArguments().First();
                var openNullableGetter = typeof(TypeTransformerRepository).GetMethod
                (
                    nameof(GetNullableTransformer),
                    BindingFlags.Instance | BindingFlags.NonPublic
                );

                // ReSharper disable once PossibleNullReferenceException
                var closedNullableGetter = openNullableGetter.MakeGenericMethod(innerType);

                return (ITypeTransformer)closedNullableGetter.Invoke(this, null);
            }

            throw new NotSupportedException("The given type doesn't have a compatible type transformer.");
        }

        /// <summary>
        /// Gets a type transformer which can transform strings to pointers and vice versa.
        /// </summary>
        /// <returns>A string transformer.</returns>
        [NotNull]
        internal StringTransformer GetStringTransformer()
        {
            return (StringTransformer)_typeTransformers[typeof(string)];
        }

        /// <summary>
        /// Gets a type transformer which can transform nullable value types to pointers and vice versa.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <returns>A nullable transformer.</returns>
        [NotNull]
        internal NullableTransformer<T> GetNullableTransformer<T>() where T : struct
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
