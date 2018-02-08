//
//  TransientMethodInfo.cs
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
using JetBrains.Annotations;

namespace AdvancedDLSupport
{
    /// <summary>
    /// Container class for transient method info, that is, a method info that may not have been constructed yet.
    /// </summary>
    internal class TransientMethodInfo
    {
        /// <summary>
        /// Gets the method info of a constructed method type.
        /// </summary>
        [CanBeNull]
        private MethodInfo Info { get; }

        /// <summary>
        /// Gets the name of the method.
        /// </summary>
        [NotNull, PublicAPI]
        public string Name { get; }

        /// <summary>
        /// Gets the return value type of the method.
        /// </summary>
        [NotNull, PublicAPI]
        public Type ReturnType { get; }

        /// <summary>
        /// Gets the parameter types of the method.
        /// </summary>
        [NotNull, PublicAPI]
        public IReadOnlyList<Type> ParameterTypes { get; }

        /// <summary>
        /// Gets the attributes of the method.
        /// </summary>
        [PublicAPI]
        public MethodAttributes Attributes { get; }

        /// <summary>
        /// Gets the parameter attributes of the method's return value.
        /// </summary>
        [PublicAPI]
        public ParameterAttributes ReturnParameterAttributes { get; }

        /// <summary>
        /// Gets the names of the method's parameters.
        /// </summary>
        [NotNull, PublicAPI]
        public IReadOnlyList<string> ParameterNames { get; }

        /// <summary>
        /// Gets the parameter attributes of the method's parameters.
        /// </summary>
        [NotNull, PublicAPI]
        public IReadOnlyList<ParameterAttributes> ParameterAttributes { get; }

        /// <summary>
        /// Gets the custom attributes applied to the method.
        /// </summary>
        [NotNull, PublicAPI]
        public IReadOnlyList<CustomAttributeData> CustomAttributes { get; }

        /// <summary>
        /// Gets the custom attributes applied to the method's return value.
        /// </summary>
        [NotNull, ItemNotNull, PublicAPI]
        public IReadOnlyList<CustomAttributeData> CustomReturnParameterAttributes { get; }

        /// <summary>
        /// Gets the custom attributes applied to the method's parameters.
        /// </summary>
        [NotNull, ItemNotNull, PublicAPI]
        public IReadOnlyList<IReadOnlyList<CustomAttributeData>> CustomParameterAttributes { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TransientMethodInfo"/> class. This constructor requires the
        /// given method info to originate from a constructed type.
        /// </summary>
        /// <param name="methodInfo">The method info to base the transient info on.</param>
        public TransientMethodInfo([NotNull] MethodInfo methodInfo)
        {
            Info = methodInfo;
            Name = Info.Name;
            ReturnType = Info.ReturnType;
            ParameterTypes = Info.GetParameters().Select(p => p.ParameterType).ToList();

            Attributes = Info.Attributes;
            CustomAttributes = Info.GetCustomAttributesData().ToList();

            ReturnParameterAttributes = Info.ReturnParameter.Attributes;
            ParameterNames = Info.GetParameters().Select(p => p.Name).ToList();
            ParameterAttributes = Info.GetParameters().Select(p => p.Attributes).ToList();

            CustomReturnParameterAttributes = Info.ReturnParameter.GetCustomAttributesData().ToList();
            CustomParameterAttributes = Info.GetParameters().Select(p => p.GetCustomAttributesData().ToList()).ToList();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TransientMethodInfo"/> class.
        /// </summary>
        /// <param name="builder">The method builer that is creating the method.</param>
        /// <param name="parameterTypes">The types of the method's parameters.</param>
        /// <param name="customAttributes">The custom attributes applied to the method.</param>
        /// <param name="returnValueAttributes">The return value attributes of the method.</param>
        /// <param name="parameterNames">The names of the parameters.</param>
        /// <param name="parameterAttributes">The parameter attributes of the method.</param>
        /// <param name="customReturnValueAttributes">The custom attributes applied to the method's return value.</param>
        /// <param name="customParameterAttributes">The custom attibutes applied to the method's parameters.</param>
        public TransientMethodInfo
        (
            [NotNull] MethodBuilder builder,
            IReadOnlyList<Type> parameterTypes,
            IReadOnlyList<CustomAttributeData> customAttributes,
            ParameterAttributes returnValueAttributes,
            IReadOnlyList<string> parameterNames,
            IReadOnlyList<ParameterAttributes> parameterAttributes,
            IReadOnlyList<CustomAttributeData> customReturnValueAttributes,
            IReadOnlyList<IReadOnlyList<CustomAttributeData>> customParameterAttributes
        )
            : this
            (
                builder.Name,
                builder.ReturnType,
                parameterTypes,
                builder.Attributes,
                customAttributes,
                returnValueAttributes,
                parameterNames,
                parameterAttributes,
                customReturnValueAttributes,
                customParameterAttributes
            )
        {
            ParameterNames = parameterNames;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TransientMethodInfo"/> class.
        /// </summary>
        /// <param name="name">The name of the method.</param>
        /// <param name="returnType">The return type of the method.</param>
        /// <param name="parameterTypes">The types of the method's parameters.</param>
        /// <param name="attributes">The method's attributes.</param>
        /// <param name="customAttributes">The custom attributes applied to the method.</param>
        /// <param name="returnParameterAttributes">The return value attributes of the method.</param>
        /// <param name="parameterNames">The names of the parameters.</param>
        /// <param name="parameterAttributes">The parameter attributes of the method.</param>
        /// <param name="customReturnParameterAttributes">The custom attributes applied to the method's return value.</param>
        /// <param name="customParameterAttributes">The custom attibutes applied to the method's parameters.</param>
        public TransientMethodInfo
        (
            string name,
            Type returnType,
            IReadOnlyList<Type> parameterTypes,
            MethodAttributes attributes,
            IReadOnlyList<CustomAttributeData> customAttributes,
            ParameterAttributes returnParameterAttributes,
            IReadOnlyList<string> parameterNames,
            IReadOnlyList<ParameterAttributes> parameterAttributes,
            IReadOnlyList<CustomAttributeData> customReturnParameterAttributes,
            IReadOnlyList<IReadOnlyList<CustomAttributeData>> customParameterAttributes
        )
        {
            Name = name;
            ReturnType = returnType;
            ParameterTypes = parameterTypes;
            Attributes = attributes;
            CustomAttributes = customAttributes;
            ReturnParameterAttributes = returnParameterAttributes;
            ParameterNames = parameterNames;
            ParameterAttributes = parameterAttributes;
            CustomReturnParameterAttributes = customReturnParameterAttributes;
            CustomParameterAttributes = customParameterAttributes;
        }
    }
}
