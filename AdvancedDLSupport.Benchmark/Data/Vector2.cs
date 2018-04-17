//
//  Vector2.cs
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

using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;

namespace AdvancedDLSupport.Benchmark
{
    /// <summary>
    /// A 2-element vector of 32-bit floating-point values.
    /// </summary>
    [PublicAPI]
    public struct Vector2
    {
        /// <summary>
        /// The X-component of the vector.
        /// </summary>
        [PublicAPI]
        public float X;

        /// <summary>
        /// The Y-component of the vector.
        /// </summary>
        [PublicAPI]
        public float Y;

        /// <summary>
        /// Determines componentwise equality for two vectors.
        /// </summary>
        /// <param name="a">The first vector.</param>
        /// <param name="b">The second vector.</param>
        /// <returns>true if the vectors are equal, otherwise, false.</returns>
        [PublicAPI, SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator", Justification = "Direct comparison is required.")]
        public static bool operator ==(Vector2 a, Vector2 b)
        {
            return a.X == b.X && a.Y == b.Y;
        }

        /// <summary>
        /// Determines componentwise inequality for two vectors.
        /// </summary>
        /// <param name="a">The first vector.</param>
        /// <param name="b">The second vector.</param>
        /// <returns>true if the vectors are not equal, otherwise, false.</returns>
        [PublicAPI]
        public static bool operator !=(Vector2 a, Vector2 b)
        {
            return !(a == b);
        }

        /// <summary>
        /// Determines componentwise equality for the current and another matrix.
        /// </summary>
        /// <param name="other">The other matrix.</param>
        /// <returns>true if the vectors are equal, otherwise, false.</returns>
        [PublicAPI]
        public bool Equals(Vector2 other)
        {
            return X.Equals(other.X) && Y.Equals(other.Y);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            return obj is Vector2 vector2 && Equals(vector2);
        }

        /// <inheritdoc />
        [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode", Justification = "Struct is used for native interop.")]
        public override int GetHashCode()
        {
            unchecked
            {
                return (X.GetHashCode() * 397) ^ Y.GetHashCode();
            }
        }
    }
}
