//
//  Matrix2.cs
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

using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;

namespace AdvancedDLSupport.Benchmark
{
    /// <summary>
    /// A 2x2 matrix of 32-bit floating-point values.
    /// </summary>
    [PublicAPI]
    public struct Matrix2
    {
        /// <summary>
        /// The first row of the matrix.
        /// </summary>
        [PublicAPI]
        public Vector2 Row0;

        /// <summary>
        /// The second row of the matrix.
        /// </summary>
        [PublicAPI]
        public Vector2 Row1;

        /// <summary>
        /// Inverts a given by-reference <see cref="Matrix2"/>.
        /// </summary>
        /// <param name="matrix">The matrix.</param>
        [PublicAPI]
        public static void Invert(ref Matrix2 matrix)
        {
            // Calculate determinant over one
            float det = 1 / ((matrix.Row0.X * matrix.Row1.Y) - (matrix.Row0.Y * matrix.Row1.X));

            float tmpd = matrix.Row1.Y;

            // Swap d and a
            matrix.Row1.Y = matrix.Row0.X;
            matrix.Row0.X = tmpd;

            // Negate b and c
            matrix.Row0.Y = -matrix.Row0.Y;
            matrix.Row1.X = -matrix.Row1.X;

            // And multiply by the determinant modifier
            matrix.Row0.X *= det;
            matrix.Row0.Y *= det;
            matrix.Row1.X *= det;
            matrix.Row1.Y *= det;
        }

        /// <summary>
        /// Inverts a given by-value <see cref="Matrix2"/>.
        /// </summary>
        /// <param name="matrix">The matrix.</param>
        /// <returns>The inverted matrix.</returns>
        [PublicAPI, Pure]
        public static Matrix2 Invert(Matrix2 matrix)
        {
            Invert(ref matrix);
            return matrix;
        }

        /// <summary>
        /// Determines componentwise equality for two matrices.
        /// </summary>
        /// <param name="a">The first matrix.</param>
        /// <param name="b">The second matrix.</param>
        /// <returns>true if the matrices are equal, otherwise, false.</returns>
        [PublicAPI, Pure]
        public static bool operator ==(Matrix2 a, Matrix2 b)
        {
            return a.Row0 == b.Row0 && a.Row1 == b.Row1;
        }

        /// <summary>
        /// Determines componentwise inequality for two matrices.
        /// </summary>
        /// <param name="a">The first matrix.</param>
        /// <param name="b">The second matrix.</param>
        /// <returns>true if the matrices are not equal, otherwise, false.</returns>
        [PublicAPI, Pure]
        public static bool operator !=(Matrix2 a, Matrix2 b)
        {
            return !(a == b);
        }

        /// <summary>
        /// Determines componentwise equality for the current and another matrix.
        /// </summary>
        /// <param name="other">The other matrix.</param>
        /// <returns>true if the matrices are equal, otherwise, false.</returns>
        [PublicAPI, Pure]
        public bool Equals(Matrix2 other)
        {
            return Row0.Equals(other.Row0) && Row1.Equals(other.Row1);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            return obj is Matrix2 matrix2 && Equals(matrix2);
        }

        /// <inheritdoc />
        [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode", Justification = "Struct is used for native interop.")]
        public override int GetHashCode()
        {
            unchecked
            {
                return (Row0.GetHashCode() * 397) ^ Row1.GetHashCode();
            }
        }
    }
}
