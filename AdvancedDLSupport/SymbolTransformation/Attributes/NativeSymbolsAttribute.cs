//
//  NativeSymbolsAttribute.cs
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
using JetBrains.Annotations;

namespace AdvancedDLSupport
{
    /// <summary>
    /// Provides metadata information for expansion of native symbol names in an interface.
    /// </summary>
    [PublicAPI, AttributeUsage(AttributeTargets.Interface)]
    public class NativeSymbolsAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets the prefixes used for the symbols in the interface.
        /// </summary>
        [PublicAPI, NotNull]
        public string Prefix { get; set; }

        /// <summary>
        /// Gets or sets the expansion method used for symbol names in the interface.
        /// </summary>
        [PublicAPI]
        public SymbolTransformationMethod SymbolTransformationMethod { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="NativeSymbolsAttribute"/> class.
        /// </summary>
        public NativeSymbolsAttribute()
        {
            Prefix = string.Empty;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NativeSymbolsAttribute"/> class.
        /// </summary>
        /// <param name="prefix">The symbol prefix to use.</param>
        /// <param name="symbolTransformationMethod">The expansion method for symbols.</param>
        public NativeSymbolsAttribute([NotNull] string prefix, SymbolTransformationMethod symbolTransformationMethod)
        {
            Prefix = prefix;
            SymbolTransformationMethod = symbolTransformationMethod;
        }
    }
}
