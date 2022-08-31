//
//  SymbolTransformationMethod.cs
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

namespace AdvancedDLSupport
{
    /// <summary>
    /// The symbol expansion method to use. This maps directly to methods from the Humanizer library.
    /// </summary>
    public enum SymbolTransformationMethod
    {
        /// <summary>
        /// No transformation is applied.
        /// </summary>
        None,

        /// <summary>
        /// Converts the input words to UpperCamelCase, also removing underscores.
        /// </summary>
        Pascalize,

        /// <summary>
        /// Converts the input words to lowerCamelCase, also removing underscores.
        /// </summary>
        Camelize,

        /// <summary>
        /// Separates the input words with underscores, and converts all words to lowercase.
        /// </summary>
        Underscore,

        /// <summary>
        /// Separates the input words with dashes.
        /// </summary>
        Dasherize,

        /// <summary>
        /// Separates the input words with dashes, and converts all words to lowercase.
        /// </summary>
        Kebaberize
    }
}
