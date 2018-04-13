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
using System.Runtime.InteropServices;
using AdvancedDLSupport.Extensions;
using JetBrains.Annotations;

namespace AdvancedDLSupport.Reflection
{
    /// <summary>
    /// Wrapper class for <see cref="MethodInfo"/> and <see cref="MethodBuilder"/>, allowing equal compile-time
    /// introspection of their respective names, parameters, and types.
    /// </summary>
    [PublicAPI]
    public class IntrospectiveMethodInfo : IntrospectiveMemberBase<MethodInfo>
    {
        /// <summary>
        /// Gets the return type of the method.
        /// </summary>
        [PublicAPI]
        public Type ReturnType { get; }

        /// <summary>
        /// Gets the parameter types of the method.
        /// </summary>
        [PublicAPI]
        public IReadOnlyList<Type> ParameterTypes { get; }

        /// <summary>
        /// Gets a value indicating whether the name of the method is special.
        /// </summary>
        [PublicAPI]
        public bool IsSpecialName { get; }

        /// <summary>
        /// Gets a value indicating whether the method is abstract.
        /// </summary>
        [PublicAPI]
        public bool IsAbstract { get; }

        /// <summary>
        /// Gets the method attributes of the definition.
        /// </summary>
        [PublicAPI]
        public MethodAttributes Attributes { get; }

        /// <summary>
        /// Gets the parameter attributes of the return parameter.
        /// </summary>
        [PublicAPI]
        public ParameterAttributes ReturnParameterAttributes { get; }

        /// <summary>
        /// Gets the names of the parameters.
        /// </summary>
        [PublicAPI]
        public IReadOnlyList<string> ParameterNames { get; }

        /// <summary>
        /// Gets the parameter attributes of the parameter definitions.
        /// </summary>
        [PublicAPI]
        public IReadOnlyList<ParameterAttributes> ParameterAttributes { get; }

        /// <summary>
        /// Gets the custom attributes applied to the return value parameter.
        /// </summary>
        [PublicAPI]
        public IReadOnlyList<CustomAttributeData> ReturnParameterCustomAttributes { get; }

        /// <summary>
        /// Gets the custom attributes applied to the parameters.
        /// </summary>
        [PublicAPI]
        public IReadOnlyList<IReadOnlyList<CustomAttributeData>> ParameterCustomAttributes { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="IntrospectiveMethodInfo"/> class.
        /// </summary>
        /// <param name="methodInfo">The <see cref="MethodInfo"/> to wrap.</param>
        [PublicAPI]
        public IntrospectiveMethodInfo([NotNull] MethodInfo methodInfo)
            : base(methodInfo, methodInfo.CustomAttributes)
        {
            if (methodInfo is MethodBuilder)
            {
                throw new ArgumentException(nameof(methodInfo), $"Use the {nameof(MethodBuilder)} overload instead.");
            }

            ReturnType = methodInfo.ReturnType;
            ParameterTypes = methodInfo.GetParameters().Select(p => p.ParameterType).ToList();

            IsSpecialName = methodInfo.IsSpecialName;
            IsAbstract = methodInfo.IsAbstract;

            Attributes = methodInfo.Attributes;

            var parameterNames = new List<string>();
            var parameterAttributes = new List<ParameterAttributes>();
            var parameterCustomAttributes = new List<IEnumerable<CustomAttributeData>>();
            foreach (var parameter in methodInfo.GetParameters())
            {
                parameterNames.Add(parameter.Name);

                var customAttributes = new List<CustomAttributeData>(parameter.GetCustomAttributesData());
                // HACK: Mono workaround until bug is fixed
                var parameterMarshalAsAttribute = customAttributes
                .FirstOrDefault
                (
                    a =>
                        a.AttributeType == typeof(MarshalAsAttribute)
                )?.ToInstance<MarshalAsAttribute>();

                if (parameterMarshalAsAttribute is null && RuntimeInformation.FrameworkDescription.Contains("Mono"))
                {
                    parameterMarshalAsAttribute = Attribute.GetCustomAttribute(parameter, typeof(MarshalAsAttribute)) as MarshalAsAttribute;
                    if (!(parameterMarshalAsAttribute is null))
                    {
                        customAttributes.Add(parameterMarshalAsAttribute.GetAttributeData());
                    }
                }

                parameterCustomAttributes.Add(customAttributes);
                parameterAttributes.Add(parameter.Attributes);
            }

            ParameterNames = parameterNames;
            ParameterAttributes = parameterAttributes;
            ParameterCustomAttributes = parameterCustomAttributes.Select(pl => pl.ToList()).ToList();

            if (!(methodInfo.ReturnParameter is null))
            {
                ReturnParameterAttributes = methodInfo.ReturnParameter.Attributes;

                var returnCustomAttributes = new List<CustomAttributeData>(methodInfo.ReturnParameter.GetCustomAttributesData());
                ReturnParameterCustomAttributes = returnCustomAttributes;

                // HACK: Mono workaround until bug is fixed
                var returnParameterMarshalAsAttribute = methodInfo.ReturnParameter.GetCustomAttributesData()
                    .FirstOrDefault
                    (
                        a =>
                            a.AttributeType == typeof(MarshalAsAttribute)
                    )?.ToInstance<MarshalAsAttribute>();

                if (returnParameterMarshalAsAttribute is null && RuntimeInformation.FrameworkDescription.Contains("Mono"))
                {
                    returnParameterMarshalAsAttribute = Attribute.GetCustomAttribute(methodInfo.ReturnParameter, typeof(MarshalAsAttribute)) as MarshalAsAttribute;
                    if (!(returnParameterMarshalAsAttribute is null))
                    {
                        returnCustomAttributes.Add(returnParameterMarshalAsAttribute.GetAttributeData());
                    }
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IntrospectiveMethodInfo"/> class.
        /// </summary>
        /// <param name="builder">The method builder to wrap.</param>
        /// <param name="returnType">The return type of the method.</param>
        /// <param name="parameterTypes">The parameter types of the method.</param>
        /// <param name="definitionToCopyAttributesFrom">The definition to copy custom attributes from.</param>
        [PublicAPI]
        public IntrospectiveMethodInfo
        (
            [NotNull] MethodBuilder builder,
            [NotNull] Type returnType,
            [NotNull, ItemNotNull] IEnumerable<Type> parameterTypes,
            [CanBeNull] IntrospectiveMethodInfo definitionToCopyAttributesFrom = null)
            : base(builder, definitionToCopyAttributesFrom?.CustomAttributes ?? new List<CustomAttributeData>())
        {
            ReturnType = returnType;
            ParameterTypes = parameterTypes.ToList();

            // TODO: Pass in or determine if they are available
            IsSpecialName = builder.IsSpecialName;
            IsAbstract = builder.IsAbstract;

            if (!(definitionToCopyAttributesFrom is null))
            {
                // Copy attributes
                Attributes = definitionToCopyAttributesFrom.Attributes;

                ParameterNames = definitionToCopyAttributesFrom.ParameterNames;
                ParameterAttributes = definitionToCopyAttributesFrom.ParameterAttributes;
                ParameterCustomAttributes = definitionToCopyAttributesFrom.ParameterCustomAttributes;

                ReturnParameterAttributes = definitionToCopyAttributesFrom.ReturnParameterAttributes;
                ReturnParameterCustomAttributes = definitionToCopyAttributesFrom.ReturnParameterCustomAttributes;
            }
        }
    }
}
