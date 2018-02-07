//
//  IntrospectiveMethodInfo.cs
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
using System.Reflection.Emit;

namespace AdvancedDLSupport.Reflection
{
    /// <summary>
    /// Wrapper class for <see cref="MethodInfo"/> and <see cref="MethodBuilder"/>, allowing equal compile-time
    /// introspection of their respective names, parameters, and types.
    /// </summary>
    public class IntrospectiveMethodInfo : IntrospectiveMemberBase<MethodInfo>
    {
        /// <summary>
        /// Gets the return type of the method.
        /// </summary>
        public Type ReturnType { get; }

        /// <summary>
        /// Gets the parameter types of the method.
        /// </summary>
        public IReadOnlyList<Type> ParameterTypes { get; }

        /// <summary>
        /// Gets a value indicating whether the name of the method is special.
        /// </summary>
        public bool IsSpecialName { get; }

        /// <summary>
        /// Gets a value indicating whether the method is abstract.
        /// </summary>
        public bool IsAbstract { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="IntrospectiveMethodInfo"/> class.
        /// </summary>
        /// <param name="methodInfo">The <see cref="MethodInfo"/> to wrap.</param>
        public IntrospectiveMethodInfo(MethodInfo methodInfo)
            : base(methodInfo)
        {
            if (methodInfo is MethodBuilder)
            {
                throw new ArgumentException(nameof(methodInfo), $"Use the {nameof(MethodBuilder)} overload instead.");
            }

            ReturnType = methodInfo.ReturnType;
            ParameterTypes = methodInfo.GetParameters().Select(p => p.ParameterType).ToList();

            IsSpecialName = methodInfo.IsSpecialName;
            IsAbstract = methodInfo.IsAbstract;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IntrospectiveMethodInfo"/> class.
        /// </summary>
        /// <param name="builder">The method builder to wrap.</param>
        /// <param name="parameterTypes">The parameter types of the method.</param>
        /// <param name="returnType">The return type of the method.</param>
        /// <param name="customAttributeDatas">The custom attributes applied to the method.</param>
        public IntrospectiveMethodInfo
        (
            MethodBuilder builder,
            IEnumerable<Type> parameterTypes,
            Type returnType,
            IEnumerable<CustomAttributeData> customAttributeDatas = default
        )
            : base(builder, customAttributeDatas)
        {
            ReturnType = returnType;
            ParameterTypes = parameterTypes.ToList();

            // TODO: Pass in or determine if they are available
            IsSpecialName = builder.IsSpecialName;
            IsAbstract = builder.IsAbstract;
        }
    }
}
