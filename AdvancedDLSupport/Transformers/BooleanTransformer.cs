//
//  BooleanTransformer.cs
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

namespace AdvancedDLSupport
{
    /// <summary>
    /// Handles transformation of boolean values.
    /// </summary>
    public class BooleanTransformer : ITypeTransformer<bool, byte>
    {
        /// <inheritdoc />
        public Type LowerType()
        {
            return typeof(byte);
        }

        /// <inheritdoc />
        public Type RaiseType()
        {
            return typeof(bool);
        }

        /// <inheritdoc />
        public byte LowerValue(bool value)
        {
            return value ? (byte)1 : (byte)0;
        }

        /// <inheritdoc />
        public bool RaiseValue(byte value)
        {
            return value > 0;
        }
    }
}
