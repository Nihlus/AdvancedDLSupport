//
//  IntrospectivePropertyInfo.cs
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
using JetBrains.Annotations;

namespace AdvancedDLSupport.Reflection
{
    /// <summary>
    /// Wrapper class for property infos.
    /// </summary>
    [PublicAPI]
    public class IntrospectivePropertyInfo : IntrospectiveMemberBase<PropertyInfo>
    {
        /// <summary>
        /// Gets the type of the property.
        /// </summary>
        [PublicAPI]
        public Type PropertyType { get; }

        /// <summary>
        /// Gets the index parameter types of the property.
        /// </summary>
        [PublicAPI]
        public IReadOnlyList<Type> IndexParameterTypes { get; }

        /// <summary>
        /// Gets a value indicating whether the property can be read.
        /// </summary>
        [PublicAPI]
        public bool CanRead { get; }

        /// <summary>
        /// Gets a value indicating whether the property can be written.
        /// </summary>
        [PublicAPI]
        public bool CanWrite { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="IntrospectivePropertyInfo"/> class.
        /// </summary>
        /// <param name="memberInfo">The property info to wrap.</param>
        /// <param name="metadataType">The type that the member gets native metadata from.</param>
        [PublicAPI]
        public IntrospectivePropertyInfo([NotNull] PropertyInfo memberInfo, [NotNull] Type metadataType)
            : base(memberInfo, metadataType)
        {
            PropertyType = memberInfo.PropertyType;
            IndexParameterTypes = memberInfo.GetIndexParameters().Select(p => p.ParameterType).ToList();
            CanRead = memberInfo.CanRead;
            CanWrite = memberInfo.CanWrite;
        }

        /// <summary>
        /// Determines whether or not the current instance has the same signature as another.
        /// </summary>
        /// <param name="other">The other property info.</param>
        /// <returns>true if the signatures are the same; otherwise, false.</returns>
        public bool HasSameSignatureAs([NotNull] IntrospectivePropertyInfo other)
        {
            if (Name != other.Name)
            {
                return false;
            }

            if (PropertyType != other.PropertyType)
            {
                return false;
            }

            return true;
        }
    }
}
