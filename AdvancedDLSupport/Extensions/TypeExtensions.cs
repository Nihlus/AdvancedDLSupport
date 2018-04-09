//
//  TypeExtensions.cs
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
using System.Reflection;
using AdvancedDLSupport.Reflection;
using JetBrains.Annotations;

namespace AdvancedDLSupport.Extensions
{
    /// <summary>
    /// Extension methods for the <see cref="Type"/> class.
    /// </summary>
    internal static class TypeExtensions
    {
        /// <summary>
        /// Gets the methods defined in the given type as wrapped introspective methods.
        /// </summary>
        /// <param name="this">The type to inspect.</param>
        /// <param name="flattenHierarchy">Whether or not the hierarchy of the type should be flattened when scanning.</param>
        /// <returns>The methods.</returns>
        [Pure, NotNull, ItemNotNull]
        public static IEnumerable<IntrospectiveMethodInfo> GetIntrospectiveMethods([NotNull] this Type @this, bool flattenHierarchy = false)
        {
            var basicBindingFlags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static;
            var flattenedBindingFlags = basicBindingFlags | BindingFlags.FlattenHierarchy;

            var bindingFlags = flattenHierarchy ? flattenedBindingFlags : basicBindingFlags;

            var methods = @this.GetMethods(bindingFlags);
            foreach (var method in methods)
            {
                yield return new IntrospectiveMethodInfo(method);
            }

            if (!@this.IsInterface || !flattenHierarchy)
            {
                yield break;
            }

            foreach (var inf in @this.GetInterfaces())
            {
                var interfaceMethods = inf.GetMethods(bindingFlags);
                foreach (var method in interfaceMethods)
                {
                    yield return new IntrospectiveMethodInfo(method);
                }
            }
        }

        /// <summary>
        /// Gets a method defined in the given type by its name and parameter types.
        /// </summary>
        /// <param name="this">The type to inspect.</param>
        /// <param name="name">The name of the method.</param>
        /// <param name="parameterTypes">The parameter types of the method.</param>
        /// <returns>The method.</returns>
        [Pure, CanBeNull]
        public static IntrospectiveMethodInfo GetIntrospectiveMethod
        (
            [NotNull] this Type @this,
            [NotNull] string name,
            [NotNull, ItemNotNull] Type[] parameterTypes
        )
        {
            var method = @this.GetMethod(name, parameterTypes);
            return method is null ? null : new IntrospectiveMethodInfo(method);
        }

        /// <summary>
        /// Checks if the give type implements a given interface type.
        /// </summary>
        /// <param name="this">The type.</param>
        /// <typeparam name="T">The interface.</typeparam>
        /// <exception cref="ArgumentException">Thrown if the interface type is not an interface.</exception>
        /// <returns>true if the type implements the interface; otherwise, false.</returns>
        [Pure]
        public static bool HasInterface<T>([NotNull] this Type @this) where T : class
        {
            if (!typeof(T).IsInterface)
            {
                throw new ArgumentException($"The type {typeof(T).Name} was not an interface.", nameof(T));
            }

            return !(@this.GetInterface(typeof(T).Name) is null);
        }

        /// <summary>
        /// Checks if the give type implements a given interface type.
        /// </summary>
        /// <param name="this">The type.</param>
        /// <typeparam name="T">The interface.</typeparam>
        /// <exception cref="ArgumentException">Thrown if the interface type is not an interface.</exception>
        /// <returns>true if the type implements the interface; otherwise, false.</returns>
        [Pure]
        public static bool HasInterface<T>([NotNull] this Type @this) where T : class
        {
            if (!typeof(T).IsInterface)
            {
                throw new ArgumentException($"The type {typeof(T).Name} was not an interface.", nameof(T));
            }

            return !(@this.GetInterface(typeof(T).Name) is null);
        }

        /// <summary>
        /// Determines whether or not the given type is a <see cref="Nullable{T}"/> that is not passed by reference.
        /// </summary>
        /// <param name="this">The type.</param>
        /// <returns>true if it is a nullable that is not passed by reference; otherwise, false.</returns>
        [Pure]
        public static bool IsNonRefNullable([NotNull] this Type @this)
        {
            if (@this.IsByRef)
            {
                return false;
            }

            return @this.IsGenericType &&
                   @this.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        /// <summary>
        /// Determines whether or not the given type is a <see cref="Nullable{T}"/> passed by reference.
        /// </summary>
        /// <param name="this">The type.</param>
        /// <returns>true if it is a nullable passed by reference; otherwise, false.</returns>
        [Pure]
        public static bool IsRefNullable([NotNull] this Type @this)
        {
            if (!@this.IsByRef)
            {
                return false;
            }

            var underlying = @this.GetElementType();

            if (underlying is null)
            {
                return false;
            }

            return underlying.IsGenericType &&
                   underlying.GetGenericTypeDefinition() == typeof(Nullable<>);
        }
    }
}
