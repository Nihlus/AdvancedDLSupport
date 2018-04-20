//
//  AttributeParserTests.cs
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
using System.Linq;
using Mono.DllMap.Extensions;
using Mono.DllMap.Tests.Data;
using Mono.DllMap.Utility;
using Xunit;
using static Mono.DllMap.Tests.Data.TestEnum;

#pragma warning disable SA1600, CS1591

namespace Mono.DllMap.Tests.Unit
{
    public class AttributeParserTests
    {
        private const string NonNegatedAttributeList = "foo,bar";
        private const string NegatedAttributeList = "!foo,bar";

        [Fact]
        public void CanParseNonNegatedAttributeList()
        {
            var actual = DllMapAttributeParser.Parse<TestEnum>(NonNegatedAttributeList);

            Assert.True(actual.HasFlagFast(Foo));
            Assert.True(actual.HasFlagFast(Bar));
            Assert.False(actual.HasFlagFast(Baz));
        }

        [Fact]
        public void CanParseNegatedAttributeList()
        {
            var actual = DllMapAttributeParser.Parse<TestEnum>(NegatedAttributeList);

            Assert.False(actual.HasFlagFast(Foo));
            Assert.False(actual.HasFlagFast(Bar));
            Assert.True(actual.HasFlagFast(Baz));
        }

        [Fact]
        public void AttributeParserThrowsIfPassedNonEnumType()
        {
            Assert.Throws<ArgumentException>
            (
                () =>
                    DllMapAttributeParser.Parse<int>(NonNegatedAttributeList)
            );
        }

        [Fact]
        public void AttributeParserThrowsIfPassedEnumTypeWithoutFlagAttribute()
        {
            Assert.Throws<ArgumentException>
            (
                () =>
                    DllMapAttributeParser.Parse<TestEnumWithoutFlagAttribute>(NonNegatedAttributeList)
            );
        }

        [Fact]
        public void AttributeParserReturnsAllPossibleValuesForNullInput()
        {
            var expected = Enum.GetValues(typeof(TestEnum)).Cast<TestEnum>().Aggregate((a, b) => a | b);
            var actual = DllMapAttributeParser.Parse<TestEnum>(null);

            Assert.Equal(expected, actual);
        }
    }
}
