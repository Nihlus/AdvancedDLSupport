//
//  LibraryIdentifierEqualityComparer.cs
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

using System.Collections.Generic;

namespace AdvancedDLSupport
{
    /// <summary>
    /// Compares library identifiers for equality.
    /// </summary>
    internal class LibraryIdentifierEqualityComparer : IEqualityComparer<GeneratedImplementationTypeIdentifier>
    {
        /// <inheritdoc />
        public bool Equals(GeneratedImplementationTypeIdentifier x, GeneratedImplementationTypeIdentifier y)
        {
            return x.Equals(y);
        }

        /// <inheritdoc />
        public int GetHashCode(GeneratedImplementationTypeIdentifier obj)
        {
            return obj.GetHashCode();
        }
    }
}
