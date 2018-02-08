//
//  CustomAttributeDataExtensions.cs
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
using System.Reflection.Emit;
using JetBrains.Annotations;

namespace AdvancedDLSupport.Extensions
{
    /// <summary>
    /// Extension methods for the <see cref="CustomAttributeData"/> class.
    /// </summary>
    internal static class CustomAttributeDataExtensions
    {
        /// <summary>
        /// Gets an attribute builder for the given attribute data instance.
        /// </summary>
        /// <param name="this">The attribute data to create a builder for.</param>
        /// <returns>An attribute builder.</returns>
        [NotNull, Pure]
        public static CustomAttributeBuilder GetAttributeBuilder([NotNull] this CustomAttributeData @this)
        {
            return new CustomAttributeBuilder
            (
                @this.Constructor,
                @this.ConstructorArguments.Select(a => a.Value).ToArray()
            );
        }
    }
}
