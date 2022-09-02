//
//  AttributePassthroughTests.cs
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

public class AttributePassthroughTests : LibraryTestBase<IAttributePassthroughLibrary>
{
    private const string _libraryName = nameof(AttributePassthroughTests);

    public AttributePassthroughTests()
        : base(_libraryName)
    {
    }

    [Fact]
    public void RespectsReturnValueMarshalAs()
    {
        Assert.True(Library.CheckIfGreaterThanZero(1));
        Assert.False(Library.CheckIfGreaterThanZero(0));
        Assert.True(Library.CheckIfStringIsNull(null));
        Assert.False(Library.CheckIfStringIsNull("Hello!"));
    }

    [Fact]
    public void RespectsStringMarshallingAttribute()
    {
        var expected = "Äta gurka i en båt på ön";

        Assert.Equal(expected, Library.EchoWString(expected));
    }
}
