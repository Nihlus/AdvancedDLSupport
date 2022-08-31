//
//  DllImportTest.cs
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

using System.Runtime.InteropServices;
using AdvancedDLSupport.Benchmark.Data;

namespace AdvancedDLSupport.Benchmark.Native
{
    /// <summary>
    /// <see cref="DllImportAttribute"/> interop methods.
    /// </summary>
    internal static class DllImportTest
    {
        /// <summary>
        /// Inverts a given by-reference <see cref="Matrix2"/>.
        /// </summary>
        /// <param name="matrix">The matrix.</param>
        [DllImport(Program.LibraryName)]
        public static extern void InvertMatrixByPtr(ref Matrix2 matrix);

        /// <summary>
        /// Inverts a given by-value <see cref="Matrix2"/>.
        /// </summary>
        /// <param name="matrix">The matrix.</param>
        /// <returns>The inverted matrix.</returns>
        [DllImport(Program.LibraryName)]
        public static extern Matrix2 InvertMatrixByValue(Matrix2 matrix);
    }
}
