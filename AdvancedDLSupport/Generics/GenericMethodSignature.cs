//
//  GenericMethodSignature.cs
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
using AdvancedDLSupport.Reflection;
using JetBrains.Annotations;

namespace AdvancedDLSupport.Generics
{
    /// <summary>
    /// Holds the closed signature of a generic method, allowing it to be used as a lookup key.
    /// </summary>
    public class GenericMethodSignature : IEquatable<GenericMethodSignature>
    {
        /// <summary>
        /// Gets the name of the method.
        /// </summary>
        [NotNull]
        public string Name { get; }

        /// <summary>
        /// Gets the return type of the method.
        /// </summary>
        [NotNull]
        public Type ReturnType { get; }

        /// <summary>
        /// Gets the parameter types of the method, if any.
        /// </summary>
        [NotNull]
        public IReadOnlyList<Type> ParameterTypes { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericMethodSignature"/> class.
        /// </summary>
        /// <param name="methodInfo">The method info to create the signature from.</param>
        public GenericMethodSignature([NotNull] IntrospectiveMethodInfo methodInfo)
            : this(methodInfo.Name, methodInfo.ReturnType, methodInfo.ParameterTypes)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericMethodSignature"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="returnType">The return type.</param>
        /// <param name="parameterTypes">The parameter types.</param>
        public GenericMethodSignature([NotNull] string name, [NotNull] Type returnType, [CanBeNull] IReadOnlyList<Type> parameterTypes = null)
        {
            Name = name;
            ReturnType = returnType;
            ParameterTypes = parameterTypes ?? new List<Type>();
        }

        /// <inheritdoc/>
        public bool Equals(GenericMethodSignature other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return string.Equals(Name, other.Name) && ReturnType == other.ReturnType && ParameterTypes.Equals(other.ParameterTypes);
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != GetType())
            {
                return false;
            }

            return Equals((GenericMethodSignature)obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Name.GetHashCode();
                hashCode = (hashCode * 397) ^ ReturnType.GetHashCode();
                hashCode = (hashCode * 397) ^ ParameterTypes.Aggregate(hashCode, (seed, type) => (seed * 397) ^ type.GetHashCode());
                return hashCode;
            }
        }

        /// <summary>
        /// Compares two signatures for equality.
        /// </summary>
        /// <param name="left">The first signature.</param>
        /// <param name="right">The second signature.</param>
        /// <returns>true if the signatures are equal; otherwise, false.</returns>
        public static bool operator ==([CanBeNull] GenericMethodSignature left, [CanBeNull] GenericMethodSignature right)
        {
            return Equals(left, right);
        }

        /// <summary>
        /// Compares two signatures for inequality.
        /// </summary>
        /// <param name="left">The first signature.</param>
        /// <param name="right">The second signature.</param>
        /// <returns>true if the signatures are not equal; otherwise, false.</returns>
        public static bool operator !=([CanBeNull] GenericMethodSignature left, [CanBeNull] GenericMethodSignature right)
        {
            return !Equals(left, right);
        }
    }
}
