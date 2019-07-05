//
//  IndirectCallsTestBase.cs
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

using AdvancedDLSupport.Tests.Data;
using Xunit;

#pragma warning disable SA1600, CS1591

namespace AdvancedDLSupport.Tests.TestBases
{
    public abstract class IndirectCallsTestBase<T> : LibraryTestBase<T> where T : class, IIndirectCallLibrary
    {
        private const string LibraryName = "IndirectCallTests";

        protected IndirectCallsTestBase()
            : base(LibraryName)
        {
        }

        protected override ImplementationOptions GetImplementationOptions()
        {
            return ImplementationOptions.UseIndirectCalls;
        }

        [Fact]
        public void CanCallSimpleFunction()
        {
            var result = Library.Multiply(5, 5);

            Assert.Equal(25, result);
        }

        [Fact]
        public void CanCallFunctionWithByRefParameter()
        {
            var data = new TestStruct { A = 5, B = 15 };
            var result = Library.GetStructAValueByRef(ref data);

            Assert.Equal(data.A, result);
        }

        [Fact]
        public void CanCallFunctionWithByInParameter()
        {
            var data = new TestStruct { A = 5, B = 15 };
            var result = Library.GetStructAValueByIn(data);

            Assert.Equal(data.A, result);
        }

        [Fact]
        public void CanCallFunctionWithByValueParameter()
        {
            var data = new TestStruct { A = 5, B = 15 };
            var result = Library.GetStructAValueByValue(data);

            Assert.Equal(data.A, result);
        }

        [Fact]
        public void CanCallFunctionWithByRefReturnValue()
        {
            const int a = 5;
            const int b = 15;

            ref var result = ref Library.GetInitializedStructByRef(a, b);

            Assert.Equal(a, result.A);
            Assert.Equal(b, result.B);
        }

        [Fact]
        public void CanCallFunctionWithByValueReturnValue()
        {
            const int a = 5;
            const int b = 15;

            var result = Library.GetInitializedStructByValue(a, b);

            Assert.Equal(a, result.A);
            Assert.Equal(b, result.B);
        }

        [Fact]
        public void CanCallFunctionWithNullableReturnValue()
        {
            var result = Library.GetNullTestStruct();

            Assert.Null(result);
        }

        [Fact]
        public void CanCallFunctionWithNullableParameter()
        {
            var resultNull = Library.IsTestStructNull(null);

            var strct = new TestStruct { A = 5, B = 15 };
            var resultNotNull = Library.IsTestStructNull(strct);

            Assert.True(resultNull);
            Assert.False(resultNotNull);
        }
    }
}
