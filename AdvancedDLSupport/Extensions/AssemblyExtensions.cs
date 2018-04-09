//
//  AssemblyExtensions.cs
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
using System.Reflection;
using JetBrains.Annotations;

namespace AdvancedDLSupport.Extensions
{
    /// <summary>
    /// Extension methods for the <see cref="MemberInfo"/> class.
    /// </summary>
    internal static class AssemblyExtensions
    {
        /// <summary>
        /// Determines whether or not the given member has a custom attribute of the given type.
        /// </summary>
        /// <param name="this">The member info.</param>
        /// <typeparam name="T">The attribute type.</typeparam>
        /// <returns>true if it has one; otherwise, false.</returns>
        public static bool HasCustomAttribute<T>([NotNull] this Assembly @this) where T : Attribute
        {
            return !(@this.GetCustomAttribute<T>() is null);
        }
    }
}
