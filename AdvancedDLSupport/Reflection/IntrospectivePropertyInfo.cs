//
//  IntrospectivePropertyInfo.cs
//
//  Copyright (c) 2018 Firwood Software
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AdvancedDLSupport.Reflection
{
    /// <summary>
    /// Wrapper class for property infos.
    /// </summary>
    public class IntrospectivePropertyInfo : IntrospectiveMemberBase<PropertyInfo>
    {
        /// <summary>
        /// Gets the type of the property.
        /// </summary>
        public Type PropertyType { get; }

        /// <summary>
        /// Gets the index parameter types of the property.
        /// </summary>
        public IReadOnlyList<Type> IndexParameterTypes { get; }

        /// <summary>
        /// Gets a value indicating whether the property can be read.
        /// </summary>
        public bool CanRead { get; }

        /// <summary>
        /// Gets a value indicating whether the property can be written.
        /// </summary>
        public bool CanWrite { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="IntrospectivePropertyInfo"/> class.
        /// </summary>
        /// <param name="memberInfo">The property info to wrap.</param>
        public IntrospectivePropertyInfo(PropertyInfo memberInfo)
            : base(memberInfo)
        {
            PropertyType = memberInfo.PropertyType;
            IndexParameterTypes = memberInfo.GetIndexParameters().Select(p => p.ParameterType).ToList();
            CanRead = memberInfo.CanRead;
            CanWrite = memberInfo.CanWrite;
        }
    }
}
