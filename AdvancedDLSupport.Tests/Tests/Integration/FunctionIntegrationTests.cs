//
//  FunctionIntegrationTests.cs
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
    public class FunctionIntegrationTests : LibraryTestBase<IFunctionLibrary>
    {
        private const string LibraryName = "FunctionTests";

        public FunctionIntegrationTests()
            : base(LibraryName)
        {
        }

        [Fact]
        public void CanCallFunctionWithStructParameter()
        {
            var value = 5;
            var multiplier = 15;

            var strct = new TestStruct { A = value };

            var expected = value * multiplier;
            var actual = Library.DoStructMath(ref strct, multiplier);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void CanCallFunctionWithSimpleParameter()
        {
            var value = 5;
            var multiplier = 15;

            var expected = value * multiplier;
            var actual = Library.Multiply(value, multiplier);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void CanCallFunctionWithDifferentEntryPoint()
        {
            var value = 5;
            var multiplier = 15;

            var strct = new TestStruct { A = value };

            var expected = value * multiplier;
            var actual = Library.Multiply(ref strct, multiplier);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void CanCallFunctionWithDifferentCallingConvention()
        {
            var value = 5;
            var other = 15;

            var expected = value - other;
            var actual = Library.STDCALLSubtract(value, other);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void CanCallDuplicateFunction()
        {
            var value = 5;
            var other = 15;

            var expected = value - other;
            var actual = Library.DuplicateSubtract(value, other);

            Assert.Equal(expected, actual);
        }
    }
}
