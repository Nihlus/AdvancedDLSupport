//
//  GenericFunctionTests.cs
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

using System;
using AdvancedDLSupport.Tests.Data;
using AdvancedDLSupport.Tests.TestBases;
using Xunit;

#pragma warning disable SA1600, CS1591

namespace AdvancedDLSupport.Tests.Integration
{
    public class GenericFunctionTests
    {
        private const string LibraryName = "GenericFunctionTests";

        public class GenericParameters : LibraryTestBase<IGenericFunctionLibrary>
        {
            public GenericParameters()
                : base(LibraryName)
            {
            }

            [Fact]
            public void CanGetSizeOfGenericFloat()
            {
                Assert.Equal(new UIntPtr(4), Library.GetSize(0.0f));
            }

            [Fact]
            public void CanGetSizeOfGenericDouble()
            {
                Assert.Equal(new UIntPtr(8), Library.GetSize(0.0d));
            }

            [Fact]
            public void CanGetSizeOfGenericByte()
            {
                Assert.Equal(new UIntPtr(1), Library.GetSize<byte>(0));
            }

            [Fact]
            public void CanGetSizeOfGenericShort()
            {
                Assert.Equal(new UIntPtr(2), Library.GetSize<short>(0));
            }

            [Fact]
            public void CanGetSizeOfGenericInt()
            {
                Assert.Equal(new UIntPtr(4), Library.GetSize<int>(0));
            }

            [Fact]
            public void CanGetSizeOfGenericStruct()
            {
                var testStruct = default(TestStruct);
                Assert.Equal(new UIntPtr(8), Library.GetSize<TestStruct>(testStruct));
            }
        }

        public class GenericReturnParameters : LibraryTestBase<IGenericFunctionLibrary>
        {
            public GenericReturnParameters()
                : base(LibraryName)
            {
            }

            [Fact]
            public void CanGetGenericFloatValue()
            {
                Assert.Equal(1.0f, Library.GetValue<float>());
            }

            [Fact]
            public void CanGetGenericDoubleValue()
            {
                Assert.Equal(2.0d, Library.GetValue<double>());
            }

            [Fact]
            public void CanGetGenericByteValue()
            {
                Assert.Equal(4, Library.GetValue<byte>());
            }

            [Fact]
            public void CanGetGenericShortValue()
            {
                Assert.Equal(8, Library.GetValue<short>());
            }

            [Fact]
            public void CanGetGenericIntValue()
            {
                Assert.Equal(16, Library.GetValue<int>());
            }

            [Fact]
            public void CanGetGenericStructValue()
            {
                var result = Library.GetValue<TestStruct>();
                Assert.Equal(32, result.A);
                Assert.Equal(64, result.B);
            }
        }
    }
}
