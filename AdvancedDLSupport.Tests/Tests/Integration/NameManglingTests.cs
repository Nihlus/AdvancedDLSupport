//
//  NameManglingTests.cs
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

using AdvancedDLSupport.Tests.Data;
using AdvancedDLSupport.Tests.TestBases;
using Xunit;

#pragma warning disable SA1600, CS1591

namespace AdvancedDLSupport.Tests.Integration
{
    public class NameManglingTests : LibraryTestBase<INameManglingTests>
    {
        private const string LibraryName = "NameManglingTests";

        public NameManglingTests()
            : base(LibraryName)
        {
        }

        [Fact]
        public void CanMangleSimpleFunction()
        {
            var a = 5;
            var b = 15;

            var expected = a * b;

            var actual = Library.Multiply(a, b);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void CanMangleFunctionWithStructByVal()
        {
            var a = 5;
            var b = 15;

            var expected = a * b;
            var value = new TestStruct { A = a, B = b };

            var actual = Library.MultiplyStructByVal(value);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void CanMangleFunctionWithStructByRef()
        {
            var a = 5;
            var b = 15;

            var expected = a * b;
            var value = new TestStruct { A = a, B = b };

            var actual = Library.MultiplyStructByRef(ref value);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public unsafe void CanMangleFunctionWithStructByPtr()
        {
            var a = 5;
            var b = 15;

            var expected = a * b;
            var value = new TestStruct { A = a, B = b };

            var actual = Library.MultiplyStructByPtr(&value);

            Assert.Equal(expected, actual);
        }
    }
}
