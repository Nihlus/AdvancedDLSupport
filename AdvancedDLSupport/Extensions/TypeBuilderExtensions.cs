//
//  TypeBuilderExtensions.cs
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
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using AdvancedDLSupport.Reflection;

namespace AdvancedDLSupport.Extensions
{
    /// <summary>
    /// Extensions for the <see cref="TypeBuilder"/> class.
    /// </summary>
    public static class TypeBuilderExtensions
    {
        /// <summary>
        /// Defines a new method in the type, based on the <paramref name="baseDefinition"/>.
        /// </summary>
        /// <param name="this">The type to define the method in.</param>
        /// <param name="baseDefinition">The definition to base the new method on.</param>
        /// <param name="newAttributes">The new method attributes.</param>
        /// <param name="newReturnType">The new return type of the method.</param>
        /// <param name="newParameterTypes">The new parameter types of the method.</param>
        /// <param name="definitionToCopyAttributesFrom">The definition to copy custom attributes from.</param>
        /// <returns>An introspective definition.</returns>
        public static IntrospectiveMethodInfo DefineMethod
        (
            this TypeBuilder @this,
            IntrospectiveMethodInfo baseDefinition,
            MethodAttributes newAttributes = 0,
            Type newReturnType = null,
            Type[] newParameterTypes = null,
            IntrospectiveMethodInfo definitionToCopyAttributesFrom = null
        )
        {
            newAttributes = newAttributes == 0 ? baseDefinition.Attributes : newAttributes;
            newReturnType = newReturnType ?? baseDefinition.ReturnType;
            newParameterTypes = newParameterTypes ?? baseDefinition.ParameterTypes.ToArray();

            var methodBuilder = @this.DefineMethod
            (
                baseDefinition.Name,
                newAttributes,
                newReturnType,
                newParameterTypes
            );

            if (!(definitionToCopyAttributesFrom is null))
            {
                methodBuilder.ApplyCustomAttributesFrom(definitionToCopyAttributesFrom, newReturnType, newParameterTypes);
                return new IntrospectiveMethodInfo(methodBuilder, newReturnType, newParameterTypes, definitionToCopyAttributesFrom);
            }

            return new IntrospectiveMethodInfo(methodBuilder, newReturnType, newParameterTypes);
        }
    }
}
