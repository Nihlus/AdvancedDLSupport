//
//  IntrospectiveMethodInfo.cs
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
        /// Gets the return parameter required modifiers of the method.
        /// </summary>
        [PublicAPI]
        public Type[] ReturnParameterRequiredModifiers { get; }

        /// <summary>
        /// Gets the return parameter optional modifiers of the method.
        /// </summary>
        [PublicAPI]
        public Type[] ReturnParameterOptionalModifiers { get; }

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
        /// Gets the required modifiers of the parameters.
        /// </summary>
        [PublicAPI]
        public IReadOnlyList<Type[]> ParameterRequiredModifiers { get; }

        /// <summary>
        /// Gets the required modifiers of the parameters.
        /// </summary>
        [PublicAPI]
        public IReadOnlyList<Type[]> ParameterOptionalModifiers { get; }

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
        /// <param name="metadataType">The type that the member gets native metadata from.</param>
        [PublicAPI]
        public IntrospectiveMethodInfo(MethodInfo methodInfo, Type metadataType)
            : base(methodInfo, metadataType, methodInfo.CustomAttributes)
        {
            if (methodInfo is MethodBuilder)
            {
                throw new ArgumentException(nameof(methodInfo), $"Use the {nameof(MethodBuilder)} overload instead.");
            }

            ReturnType = methodInfo.ReturnType;
            ReturnParameterRequiredModifiers = methodInfo.ReturnParameter.GetRequiredCustomModifiers();
            ReturnParameterOptionalModifiers = methodInfo.ReturnParameter.GetOptionalCustomModifiers();

            IsSpecialName = methodInfo.IsSpecialName;
            IsAbstract = methodInfo.IsAbstract;

            Attributes = methodInfo.Attributes;

            var parameterNames = new List<string>();
            var parameterTypes = new List<Type>();
            var parameterRequiredModifiers = new List<Type[]>();
            var parameterOptionalModifiers = new List<Type[]>();
            var parameterAttributes = new List<ParameterAttributes>();
            var parameterCustomAttributes = new List<IEnumerable<CustomAttributeData>>();
            foreach (var parameter in methodInfo.GetParameters())
            {
                parameterNames.Add(parameter.Name);
                parameterTypes.Add(parameter.ParameterType);
                parameterRequiredModifiers.Add(parameter.GetRequiredCustomModifiers());
                parameterOptionalModifiers.Add(parameter.GetOptionalCustomModifiers());

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
                    parameterMarshalAsAttribute = Attribute.GetCustomAttribute
                    (
                        parameter,
                        typeof(MarshalAsAttribute)
                    ) as MarshalAsAttribute;

                    if (!(parameterMarshalAsAttribute is null))
                    {
                        customAttributes.Add(parameterMarshalAsAttribute.GetAttributeData());
                    }
                }

                parameterCustomAttributes.Add(customAttributes);
                parameterAttributes.Add(parameter.Attributes);
            }

            ParameterTypes = parameterTypes;
            ParameterNames = parameterNames;
            ParameterRequiredModifiers = parameterRequiredModifiers;
            ParameterOptionalModifiers = parameterOptionalModifiers;
            ParameterAttributes = parameterAttributes;
            ParameterCustomAttributes = parameterCustomAttributes.Select(pl => pl.ToList()).ToList();

            if (methodInfo.ReturnParameter is null)
            {
                ReturnParameterCustomAttributes = new List<CustomAttributeData>();

                return;
            }

            ReturnParameterAttributes = methodInfo.ReturnParameter.Attributes;

            var returnCustomAttributes = new List<CustomAttributeData>
            (
                methodInfo.ReturnParameter.GetCustomAttributesData()
            );

            ReturnParameterCustomAttributes = returnCustomAttributes;

            // HACK: Mono workaround until bug is fixed
            var returnParameterMarshalAsAttribute = methodInfo.ReturnParameter.GetCustomAttributesData()
                .FirstOrDefault
                (
                    a =>
                        a.AttributeType == typeof(MarshalAsAttribute)
                )?.ToInstance<MarshalAsAttribute>();

            if (!(returnParameterMarshalAsAttribute is null) || !RuntimeInformation.FrameworkDescription.Contains("Mono"))
            {
                return;
            }

            // Don't walk the inheritance tree of the return parameter due to a bug
            // https://stackoverflow.com/a/38759885
            returnParameterMarshalAsAttribute = Attribute.GetCustomAttribute
            (
                methodInfo.ReturnParameter, typeof(MarshalAsAttribute), false
            ) as MarshalAsAttribute;

            if (!(returnParameterMarshalAsAttribute is null))
            {
                returnCustomAttributes.Add(returnParameterMarshalAsAttribute.GetAttributeData());
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IntrospectiveMethodInfo"/> class.
        /// </summary>
        /// <param name="builder">The method builder to wrap.</param>
        /// <param name="returnType">The return type of the method.</param>
        /// <param name="parameterTypes">The parameter types of the method.</param>
        /// <param name="metadataType">The type that the member gets native metadata from.</param>
        /// <param name="definitionToCopyAttributesFrom">The definition to copy custom attributes from.</param>
        [PublicAPI]
        public IntrospectiveMethodInfo
        (
            MethodBuilder builder,
            Type returnType,
            IEnumerable<Type> parameterTypes,
            Type metadataType,
            IntrospectiveMethodInfo? definitionToCopyAttributesFrom = null
        )
            : base(builder, metadataType, definitionToCopyAttributesFrom?.CustomAttributes ?? new List<CustomAttributeData>())
        {
            ReturnType = returnType;
            ParameterTypes = parameterTypes.ToList();

            // TODO: Pass in or determine if they are available
            IsSpecialName = builder.IsSpecialName;
            IsAbstract = builder.IsAbstract;

            if (definitionToCopyAttributesFrom is null)
            {
                ParameterAttributes = new List<ParameterAttributes>();
                ParameterCustomAttributes = new List<IReadOnlyList<CustomAttributeData>>();
                ParameterNames = new List<string>();
                ReturnParameterCustomAttributes = new List<CustomAttributeData>();

                return;
            }

            // Copy attributes
            Attributes = definitionToCopyAttributesFrom.Attributes;

            ParameterNames = definitionToCopyAttributesFrom.ParameterNames;
            ParameterAttributes = definitionToCopyAttributesFrom.ParameterAttributes;
            ParameterCustomAttributes = definitionToCopyAttributesFrom.ParameterCustomAttributes;

            ReturnParameterAttributes = definitionToCopyAttributesFrom.ReturnParameterAttributes;
            ReturnParameterCustomAttributes = definitionToCopyAttributesFrom.ReturnParameterCustomAttributes;
        }

        /// <summary>
        /// Determines whether or not the current instance has the same signature as another.
        /// </summary>
        /// <param name="other">The other method info.</param>
        /// <returns>true if the signatures are the same; otherwise, false.</returns>
        public bool HasSameSignatureAs(IntrospectiveMethodInfo other)
        {
            if (Name != other.Name)
            {
                return false;
            }

            if (ReturnType != other.ReturnType)
            {
                return false;
            }

            if (ParameterTypes.Count != other.ParameterTypes.Count)
            {
                return false;
            }

            if (!ParameterTypes.SequenceEqual(other.ParameterTypes))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Determines whether or not the method's return parameter has an attribute of the given type.
        /// </summary>
        /// <typeparam name="T">The attribute type.</typeparam>
        /// <returns>true if the parameter has an attribute of the given type; otherwise, false.</returns>
        public bool ReturnParameterHasCustomAttribute<T>() where T : Attribute
        {
            return ReturnParameterCustomAttributes.Any(d => d.AttributeType == typeof(T));
        }

        /// <summary>
        /// Determines whether or not the parameter at the given index has an attribute of the given type.
        /// </summary>
        /// <param name="parameterIndex">The index of the parameter.</param>
        /// <typeparam name="T">The attribute type.</typeparam>
        /// <returns>true if the parameter has an attribute of the given type; otherwise, false.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the parameter index is out of range.</exception>
        public bool ParameterHasCustomAttribute<T>(int parameterIndex) where T : Attribute
        {
            if (parameterIndex >= ParameterCustomAttributes.Count || parameterIndex < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(parameterIndex), "Parameter index out of range.");
            }

            var parameterCustomAttributes = ParameterCustomAttributes[parameterIndex];

            return parameterCustomAttributes.Any(d => d.AttributeType == typeof(T));
        }
    }
}
