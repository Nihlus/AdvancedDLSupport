//
//  MarshalAsAttributeExtensions.cs
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
using System.Reflection;
using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace AdvancedDLSupport.Extensions
{
    /// <summary>
    /// Extensions to the <see cref="MarshalAsAttribute"/> class.
    /// </summary>
    internal static class MarshalAsAttributeExtensions
    {
        /// <summary>
        /// Gets a <see cref="CustomAttributeData"/> object that sufficiently describes a <see cref="MarshalAsAttribute"/>
        /// instance.
        /// </summary>
        /// <param name="this">The instance.</param>
        /// <returns>The data.</returns>
        [Pure, NotNull]
        public static CustomAttributeData GetAttributeData([NotNull] this MarshalAsAttribute @this)
        {
            var unmanagedType = @this.Value;

            var instance = (CustomAttributeData)Activator.CreateInstance(typeof(CustomAttributeData), true);

            var constructorBackingField = instance.GetType()
            .GetField
            (
                "ctorInfo",
                BindingFlags.Instance | BindingFlags.NonPublic
            );

            var constructor = typeof(MarshalAsAttribute).GetConstructor(new[] { typeof(UnmanagedType) });

            // ReSharper disable once PossibleNullReferenceException
            constructorBackingField.SetValue(instance, constructor);

            var constructorArgListBackingField = instance.GetType()
            .GetField
            (
                $"ctorArgs",
                BindingFlags.Instance | BindingFlags.NonPublic
            );
            var constructorArgList = new List<CustomAttributeTypedArgument>
            (
                new[]
                {
                    new CustomAttributeTypedArgument(typeof(UnmanagedType), unmanagedType)
                }
            );

            // ReSharper disable once PossibleNullReferenceException
            constructorArgListBackingField.SetValue(instance, constructorArgList);

            return instance;
        }
    }
}
