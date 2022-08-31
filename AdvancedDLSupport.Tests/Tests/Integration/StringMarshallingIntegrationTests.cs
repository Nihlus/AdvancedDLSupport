//
//  StringMarshallingIntegrationTests.cs
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

public class StringMarshallingIntegrationTests : LibraryTestBase<IStringLibrary>
{
    private const string LibraryName = "StringTests";

    public StringMarshallingIntegrationTests()
        : base(LibraryName)
    {
    }

    [Fact]
    public void CanCallFunctionWithStringReturnValue()
    {
        var actual = Library.GetString();
        Assert.Equal("Hello from C!", actual);
    }

    [Fact]
    public void CanCallFunctionWithStringReturnValueWhereResultIsNull()
    {
        Assert.Null(Library.GetNullString());
    }

    [Fact]
    public void CanCallFunctionWithStringParameter()
    {
        const string testString = "I once knew a polish audio engineer";
        var expected = testString.Length;

        Assert.Equal(expected, (long)Library.StringLength(testString).ToUInt64());
    }

    [Fact]
    public void CanCallFunctionWithStringParameterWhereParameterIsNull()
    {
        Assert.True(Library.CheckIfStringIsNull(null));
    }

    [Fact]
    public void CanCallFunctionWithBStrReturnValue()
    {
        var actual = Library.EchoBStr("Hello from C!");
        Assert.Equal("Hello from C!", actual);
    }

    [Fact]
    public void CanCallFunctionWithLPTStrReturnValue()
    {
        var actual = Library.GetLPTString();
        Assert.Equal("Hello from C!", actual);
    }

    [Fact]
    public void CanCallFunctionWithLPWStrReturnValue()
    {
        var actual = Library.GetLPWString();
        Assert.Equal("Hello from C!", actual);
    }

    [Fact]
    public void CanCallFunctionWithBStrParameter()
    {
        const string testString = "I once knew a polish audio engineer";
        var expected = testString.Length;

        Assert.Equal(expected, (long)Library.BStringLength(testString).ToUInt64());
    }

    [Fact]
    public void CanCallFunctionWithLPTStrParameter()
    {
        const string testString = "I once knew a polish audio engineer";
        var expected = testString.Length;

        Assert.Equal(expected, (long)Library.LPTStringLength(testString).ToUInt64());
    }

    [Fact]
    public void CanCallFunctionWithLPWStrParameter()
    {
        const string testString = "I once knew a polish audio engineer";
        var expected = testString.Length;

        Assert.Equal(expected, (long)Library.LPWStringLength(testString).ToUInt64());
    }

    #if NETCOREAPP || NETSTANDARD2_1
    [Fact]
    public void CanCallFunctionWithLPUTF8StrReturnValue()
    {
        var actual = Library.GetLPUTF8String();
        Assert.Equal("Hello, 🦈!", actual);
    }

    [Fact]
    public void CanCallFunctionWithLPUTF8StrParameter()
    {
        const string testString = "Växeln, hallå, hallå, hallå";
        var expected = testString.Length;

        Assert.Equal(expected, (long)Library.LPUTF8StringLength(testString).ToUInt64());
    }
    #else
        [Fact(Skip = "Unsupported on this runtime.")]
        public void CanCallFunctionWithLPUTF8StrReturnValue()
        {
        }

        [Fact(Skip = "Unsupported on this runtime.")]
        public void CanCallFunctionWithLPUTF8StrParameter()
        {
        }
    #endif

    [Fact]
    public void CanCallFunctionWithCallerFreeReturnParameter()
    {
        var actual = Library.GetStringAndFree();

        Assert.Equal("Hello from C!", actual);
    }

    [Fact]
    public void CanCallFunctionWithCallerFreeParameter()
    {
        const string testString = "I once knew a polish audio engineer";
        var expected = testString.Length;

        Assert.Equal(expected, (long)Library.GetStringLengthAndFree(testString).ToUInt64());
    }
}
