//
//  MethodBuilderExtensions.cs
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
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using AdvancedDLSupport.Reflection;
using JetBrains.Annotations;

namespace AdvancedDLSupport.Extensions
{
    /// <summary>
    /// Extension methods for the <see cref="MethodBuilder"/> class.
    /// </summary>
    internal static class MethodBuilderExtensions
    {
        /// <summary>
        /// Holds blacklisted attributes which will not be copied to their respective types.
        /// </summary>
        private static readonly IReadOnlyDictionary<Type, IReadOnlyList<Type>> AttributeBlacklist = new Dictionary<Type, IReadOnlyList<Type>>
        {
            { typeof(IntPtr), new[] { typeof(MarshalAsAttribute) } }
        };

        /// <summary>
        /// Copies all custom attributes from the given <see cref="IntrospectiveMethodInfo"/> instance. This method will redefine the
        /// return value and method parameters in order to apply the required custom attributes.
        /// </summary>
        /// <param name="this">The builder to copy the attributes to.</param>
        /// <param name="source">The method to copy the attributes from.</param>
        /// <param name="newReturnParameterType">
        /// The return type of the target method. Defaults to the source return type.
        /// </param>
        /// <param name="newParameterTypes">
        /// The parameter types of the target method. Defaults to the source parameter types.
        /// </param>
        /// <param name="returnParameterAttributeFilter">
        /// A filter predicate for the attributes being copied to the return parameter. If the predicate returns true,
        /// the attribute is not copied.
        /// </param>
        /// <param name="parameterAttributeFilter">
        /// A filter predicate for the attributes being copied to the method parameters. The predicate receives the
        /// zero-based index of the parameter. If the predicate returns true, the attribute is not copied.
        /// </param>
        public static void ApplyCustomAttributesFrom
        (
            [NotNull] this MethodBuilder @this,
            [NotNull] IntrospectiveMethodInfo source,
            [CanBeNull] Type newReturnParameterType = null,
            [CanBeNull, ItemNotNull] IReadOnlyList<Type> newParameterTypes = null,
            [CanBeNull] Func<CustomAttributeData, bool> returnParameterAttributeFilter = null,
            [CanBeNull] Func<CustomAttributeData, int, bool> parameterAttributeFilter = null
        )
        {
            newReturnParameterType = newReturnParameterType ?? source.ReturnType;
            newParameterTypes = newParameterTypes ?? source.ParameterTypes;

            returnParameterAttributeFilter = returnParameterAttributeFilter ?? (attribute => false);
            parameterAttributeFilter = parameterAttributeFilter ?? ((attribute, parameterIndex) => false);

            // Pass through all applied attributes
            var returnValueBuilder = @this.DefineParameter(0, source.ReturnParameterAttributes, null);
            foreach (var attribute in source.ReturnParameterCustomAttributes)
            {
                if (AttributeBlacklist.ContainsKey(newReturnParameterType) && AttributeBlacklist[newReturnParameterType].Contains(attribute.AttributeType))
                {
                    continue;
                }

                if (returnParameterAttributeFilter(attribute))
                {
                    continue;
                }

                returnValueBuilder.SetCustomAttribute(attribute.GetAttributeBuilder());
            }

            if (source.ParameterTypes.Any())
            {
                for (var i = 0; i < source.ParameterTypes.Count; ++i)
                {
                    var targetParameterType = newParameterTypes[i];
                    var methodParameterCustomAttributes = source.ParameterCustomAttributes[i];
                    var methodParameterAttributes = source.ParameterAttributes[i];
                    var methodParameterName = source.ParameterNames[i];

                    var parameterBuilder = @this.DefineParameter(i + 1, methodParameterAttributes, methodParameterName);
                    foreach (var attribute in methodParameterCustomAttributes)
                    {
                        if (AttributeBlacklist.ContainsKey(targetParameterType) && AttributeBlacklist[targetParameterType].Contains(attribute.AttributeType))
                        {
                            continue;
                        }

                        if (parameterAttributeFilter(attribute, i))
                        {
                            continue;
                        }

                        parameterBuilder.SetCustomAttribute(attribute.GetAttributeBuilder());
                    }
                }
            }

            foreach (var attribute in source.CustomAttributes)
            {
                @this.SetCustomAttribute(attribute.GetAttributeBuilder());
            }
        }
    }
}
