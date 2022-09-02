//
//  NullableStructTests.cs
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

namespace AdvancedDLSupport.Tests.Integration;

public class NullableStructTests : LibraryTestBase<INullableLibrary>
{
    private const string _libraryName = "NullableTests";

    public NullableStructTests()
        : base(_libraryName)
    {
    }

    [Fact]
    public void CanCallFunctionWithNullableReturnValue()
    {
        var result = Library.GetAllocatedTestStruct();
        Assert.NotNull(result);
        Assert.Equal(10, result.Value.A);
        Assert.Equal(20, result.Value.B);
    }

    [Fact]
    public void CanCallFunctionWithNullableReturnValueWhereResultIsNull()
    {
        var result = Library.GetNullTestStruct();
        Assert.Null(result);
    }

    [Fact]
    public void CanCallFunctionWithNullableParameter()
    {
        Assert.False(Library.CheckIfStructIsNull(new TestStruct { A = 10, B = 20 }));
    }

    [Fact]
    public void CanCallFunctionWithNullableParameterWhereParameterIsNull()
    {
        Assert.True(Library.CheckIfStructIsNull(null));
    }

    [Fact]
    public void CanCallFunctionWithRefNullableParameter()
    {
        TestStruct? testStruct = new TestStruct { A = 10, B = 20 };

        var result = Library.CheckIfRefStructIsNull(ref testStruct);
        Assert.False(result);
    }

    [Fact]
    public void CanCallFunctionWithRefNullableParameterWhereParameterIsNull()
    {
        TestStruct? testStruct = null;

        var result = Library.CheckIfRefStructIsNull(ref testStruct);
        Assert.True(result);
    }

    [Fact]
    public void RefNullableParameterPropagatesResultsBack()
    {
        TestStruct? testStruct = new TestStruct { A = 10, B = 20 };

        Library.SetValueInNullableRefStruct(ref testStruct);

        Assert.True(testStruct.HasValue);
        Assert.Equal(15, testStruct.Value.A);
    }

    [Fact]
    public void NativeCodeCanAccessRefNullableValues()
    {
        TestStruct? testStruct = new TestStruct { A = 10, B = 20 };

        var result = Library.GetValueInNullableRefStruct(ref testStruct);

        Assert.Equal(10, result);
    }

    [Fact]
    public void CanCallFunctionWithRefNullableParameterThatRequiresLowering()
    {
        TestStruct? testStruct = new TestStruct { A = 10, B = 20 };

        var result = Library.GetAFromStructAsString(ref testStruct);

        Assert.Equal("10", result);
    }

    [Fact]
    public void CanCallFunctionWithRefNullableParameterAndNormalParameters()
    {
        const int multiplier = 5;
        TestStruct? testStruct = new TestStruct { A = 10, B = 20 };

        var result = Library.GetAFromStructMultipliedByParameter(ref testStruct, multiplier);

        Assert.Equal(10 * multiplier, result);
    }

    [Fact]
    public void CanCallFunctionWithNullableReturnReturnValueAndCallerFreeReturnValue()
    {
        var result = Library.GetAllocatedTestStructAndFree();

        Assert.NotNull(result);
        Assert.Equal(10, result.Value.A);
        Assert.Equal(20, result.Value.B);
    }

    [Fact]
    public void CanCallFunctionWithNullableReturnReturnValueAndCallerFreeReturnValueWhereResultIsNull()
    {
        var result = Library.GetNullTestStructAndFree();

        Assert.Null(result);
    }

    [Fact]
    public void CanCallFunctionWithNullableParameterAndCallerFree()
    {
        Assert.False(Library.CheckIfStructIsNullAndFree(new TestStruct { A = 10, B = 20 }));
    }

    [Fact]
    public void CanCallFunctionWithNullableParameterAndCallerFreeWhereParameterIsNull()
    {
        Assert.True(Library.CheckIfStructIsNullAndFree(null));
    }
}
