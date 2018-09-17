//
//  BooleanMarshallingTests.cs
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
using AdvancedDLSupport.Tests.TestBases;
using Xunit;

// ReSharper disable InconsistentNaming

#pragma warning disable SA1600, CS1591

namespace AdvancedDLSupport.Tests.Integration
{
    public class BooleanMarshallingTests : LibraryTestBase<IBooleanMarshallingTests>
    {
        private const string LibraryName = "BooleanMarshallingTests";

        public BooleanMarshallingTests()
            : base(LibraryName)
        {
        }

        protected override ImplementationOptions GetImplementationOptions()
        {
            return ImplementationOptions.UseIndirectCalls;
        }

        [Fact]
        public void CanMarshalParameterAsDefault()
        {
            Assert.Equal(1, Library.IsDefaultTrue(true));
        }

        [Fact]
        public void CanMarshalParameterAsI1()
        {
            Assert.Equal(1, Library.IsSByteTrue(true));
        }

        [Fact]
        public void CanMarshalParameterAsI2()
        {
            Assert.Equal(1, Library.IsShortTrue(true));
        }

        [Fact]
        public void CanMarshalParameterAsI4()
        {
            Assert.Equal(1, Library.IsIntTrue(true));
        }

        [Fact]
        public void CanMarshalParameterAsI8()
        {
            Assert.Equal(1, Library.IsLongTrue(true));
        }

        [Fact]
        public void CanMarshalParameterAsU1()
        {
            Assert.Equal(1, Library.IsByteTrue(true));
        }

        [Fact]
        public void CanMarshalParameterAsU2()
        {
            Assert.Equal(1, Library.IsUShortTrue(true));
        }

        [Fact]
        public void CanMarshalParameterAsU4()
        {
            Assert.Equal(1, Library.IsUIntTrue(true));
        }

        [Fact]
        public void CanMarshalParameterAsU8()
        {
            Assert.Equal(1, Library.IsULongTrue(true));
        }

        [Fact]
        public void CanMarshalParameterAsBOOL()
        {
            Assert.Equal(1, Library.IsBOOLTrue(true));
        }

        [Fact]
        public void CanMarshalParameterAsVariantBool()
        {
            Assert.Equal(1, Library.IsVariantBoolTrue(true));
        }

        [Fact]
        public void CanMarshalReturnParameterAsDefault()
        {
            Assert.True(Library.GetTrueDefault());
        }

        [Fact]
        public void CanMarshalReturnParameterAsI1()
        {
            Assert.True(Library.GetTrueSByte());
        }

        [Fact]
        public void CanMarshalReturnParameterAsI2()
        {
            Assert.True(Library.GetTrueShort());
        }

        [Fact]
        public void CanMarshalReturnParameterAsI4()
        {
            Assert.True(Library.GetTrueInt());
        }

        [Fact]
        public void CanMarshalReturnParameterAsI8()
        {
            Assert.True(Library.GetTrueLong());
        }

        [Fact]
        public void CanMarshalReturnParameterAsU1()
        {
            Assert.True(Library.GetTrueByte());
        }

        [Fact]
        public void CanMarshalReturnParameterAsU2()
        {
            Assert.True(Library.GetTrueUShort());
        }

        [Fact]
        public void CanMarshalReturnParameterAsU4()
        {
            Assert.True(Library.GetTrueUInt());
        }

        [Fact]
        public void CanMarshalReturnParameterAsU8()
        {
            Assert.True(Library.GetTrueULong());
        }

        [Fact]
        public void CanMarshalReturnParameterAsBOOL()
        {
            Assert.True(Library.GetTrueBOOL());
        }

        [Fact]
        public void CanMarshalReturnParameterAsVariantBool()
        {
            Assert.True(Library.GetTrueVariantBool());
        }
    }
}
