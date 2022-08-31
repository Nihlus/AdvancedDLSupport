//
//  TypeExtensions.cs
//
//  Author:
//       Jarl Gullberg <jarl.gullberg@gmail.com>
//
//  Copyright (c) Jarl Gullberg
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
        /// A class used for testing whether a generic argument is blittable/unmanaged.
        /// </summary>
        private class UnmanagedTest<T>
            where T : unmanaged
        {
        }

        /// <summary>
        /// Determines whether the given type is blittable.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <returns>True if the type is blittable.</returns>
        public static bool IsUnmanaged([NotNull] this Type type)
        {
            try
            {
                typeof(UnmanagedTest<>).MakeGenericType(type);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Determines whether the given type is a delegate type.
        /// </summary>
        /// <param name="this">The type.</param>
        /// <returns>true if the type is a delegate type; Otherwise, false.</returns>
        public static bool IsDelegate([NotNull] this Type @this)
        {
            return typeof(Delegate).IsAssignableFrom(@this);
        }

        /// <summary>
        /// Determines whether the given type is a generic delegate type - that is, a <see cref="Func{TResult}"/> or
        /// <see cref="Action"/>.
        /// </summary>
        /// <param name="this">The type.</param>
        /// <returns>true if the type is a generic delegate type; Otherwise, false.</returns>
        public static bool IsGenericDelegate([NotNull] this Type @this)
        {
            // The parameterless action is technically not a generic type, so it'll get caught here
            if (!@this.IsGenericType)
            {
                return false;
            }

            var genericType = @this.GetGenericTypeDefinition();
            if (genericType.FullName is null)
            {
                throw new InvalidOperationException("Couldn't get the full name of the given type.");
            }

            if (genericType.IsGenericFuncDelegate())
            {
                return true;
            }

            if (genericType.IsGenericActionDelegate())
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Determines whether or not the given type is a generic <see cref="Action"/> delegate.
        /// </summary>
        /// <param name="this">The type.</param>
        /// <returns>true if the type is an action delegate; otherwise, false.</returns>
        public static bool IsGenericActionDelegate([NotNull] this Type @this)
        {
            // ReSharper disable once PossibleNullReferenceException
            var genericBaseName = @this.FullName.Split('`').First();

            if (genericBaseName == typeof(Action).FullName)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Determines whether or not the given type is a generic <see cref="Func{TResult}"/> delegate.
        /// </summary>
        /// <param name="this">The type.</param>
        /// <returns>true if the type is a func delegate; otherwise, false.</returns>
        public static bool IsGenericFuncDelegate([NotNull] this Type @this)
        {
            // ReSharper disable once PossibleNullReferenceException
            var genericBaseName = @this.FullName.Split('`').First();

            // ReSharper disable once PossibleNullReferenceException
            var funcBaseName = typeof(Func<>).FullName.Split('`').First();
            if (genericBaseName == funcBaseName)
            {
                return true;
            }

            return false;
        }

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
                yield return new IntrospectiveMethodInfo(method, @this);
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
                    yield return new IntrospectiveMethodInfo(method, @this);
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
            return method is null ? null : new IntrospectiveMethodInfo(method, @this);
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
