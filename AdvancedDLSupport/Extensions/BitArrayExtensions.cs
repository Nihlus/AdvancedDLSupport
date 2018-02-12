//
//  BitArrayExtensions.cs
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
using System.Collections;
using JetBrains.Annotations;

namespace AdvancedDLSupport.Extensions
{
    /// <summary>
    /// Extension methods for the <see cref="BitArray"/> class.
    /// </summary>
    public static class BitArrayExtensions
    {
        /// <summary>
        /// Converts the <see cref="BitArray"/> into its equivalent integer representation.
        /// </summary>
        /// <param name="this">The array.</param>
        /// <returns>An equivalent integer.</returns>
        public static int ToInt32([NotNull] this BitArray @this)
        {
            if (@this.Count > 32)
            {
                throw new ArgumentOutOfRangeException(nameof(@this), "The bit array contained more than 32 bits.");
            }

            var result = new int[1];
            @this.CopyTo(result, 0);

            return result[0];
        }
    }
}
