//
//  MethodInfoExtensions.cs
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

using System.Linq;
using System.Reflection;
using JetBrains.Annotations;

namespace AdvancedDLSupport.Extensions
{
    /// <summary>
    /// Extension methods for the <see cref="MethodInfo"/> class.
    /// </summary>
    public static class MethodInfoExtensions
    {
        /// <summary>
        /// Determines whether or not the given method is complex,
        /// </summary>
        /// <param name="this">The method to check.</param>
        /// <returns>true if the method is complex; otherwise, false.</returns>
        [PublicAPI, Pure]
        public static bool IsComplexMethod([NotNull] this MethodInfo @this)
        {
            return HasComplexParameters(@this) || HasComplexReturnValue(@this);
        }

        /// <summary>
        /// Determines whether or not the method has complex parameters.
        /// </summary>
        /// <param name="this">The method.</param>
        /// <returns>true if the method has complex parameters; otherwise, false.</returns>
        [PublicAPI, Pure]
        public static bool HasComplexParameters([NotNull] this MethodBase @this)
        {
            var parameters = @this.GetParameters();
            return parameters.Any
            (
                p =>
                    p.ParameterType.IsComplexType()
            );
        }

        /// <summary>
        /// Determines whether or not the given method has a complex return type.
        /// </summary>
        /// <param name="this">The method.</param>
        /// <returns>true if the method has a complex return value; otherwise, false.</returns>
        [PublicAPI, Pure]
        public static bool HasComplexReturnValue([NotNull] this MethodInfo @this)
        {
            return @this.ReturnType.IsComplexType();
        }
    }
}
