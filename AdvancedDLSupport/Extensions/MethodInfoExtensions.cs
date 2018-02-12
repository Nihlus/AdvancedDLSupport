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

using System;
using System.Linq;
using System.Reflection;
using AdvancedDLSupport.Reflection;
using JetBrains.Annotations;

namespace AdvancedDLSupport.Extensions
{
    /// <summary>
    /// Extension methods for the <see cref="MethodInfo"/> class.
    /// </summary>
    internal static class MethodInfoExtensions
    {
        /// <summary>
        /// Determines whether or not the given method requires a set of permutations to deal with
        /// <see cref="Nullable{T}"/> parameters passed by reference.
        /// </summary>
        /// <param name="this">The method.</param>
        /// <returns>true if the method requires permutations; otherwise, false.</returns>
        public static bool RequiresRefPermutations([NotNull] this IntrospectiveMethodInfo @this)
        {
            return @this.ParameterTypes.Any(p => p.IsRefNullable());
        }

        /// <summary>
        /// Determines whether or not the given method requires lowering, that is, whether or not any of its involved
        /// types require lowering.
        /// </summary>
        /// <param name="this">The method to check.</param>
        /// <returns>true if the method is complex; otherwise, false.</returns>
        [Pure]
        public static bool RequiresLowering([NotNull] this IntrospectiveMethodInfo @this)
        {
            return ParametersRequireLowering(@this) || ReturnValueRequiresLowering(@this);
        }

        /// <summary>
        /// Determines whether or not the method's parameters require lowering.
        /// </summary>
        /// <param name="this">The method.</param>
        /// <returns>true if the method has parameters that require lowering; otherwise, false.</returns>
        [Pure]
        public static bool ParametersRequireLowering([NotNull] this IntrospectiveMethodInfo @this)
        {
            var parameters = @this.ParameterTypes;
            return parameters.Any
            (
                p =>
                    p.RequiresLowering()
            );
        }

        /// <summary>
        /// Determines whether or not the given method has a return value that requires lowering.
        /// </summary>
        /// <param name="this">The method.</param>
        /// <returns>true if the method has a return value that requires lowering; otherwise, false.</returns>
        [Pure]
        public static bool ReturnValueRequiresLowering([NotNull] this IntrospectiveMethodInfo @this)
        {
            return @this.ReturnType.RequiresLowering();
        }
    }
}
