using System;
using System.Linq;
using Mono.DllMap.Extensions;
using Mono.DllMap.Tests.Data;
using Mono.DllMap.Utility;
using Xunit;
using static Mono.DllMap.Tests.Data.TestEnum;

namespace Mono.DllMap.Tests.Unit
{
    public class AttributeParserTests
    {
        private const string NonNegatedAttributeList = "foo,bar";
        private const string NegatedAttributeList = "!foo,bar";

        [Fact]
        void CanParseNonNegatedAttributeList()
        {
            var actual = DllMapAttributeParser.Parse<TestEnum>(NonNegatedAttributeList);

            Assert.True(actual.HasFlagFast(Foo));
            Assert.True(actual.HasFlagFast(Bar));
            Assert.False(actual.HasFlagFast(Baz));
        }

        [Fact]
        void CanParseNegatedAttributeList()
        {
            var actual = DllMapAttributeParser.Parse<TestEnum>(NegatedAttributeList);

            Assert.False(actual.HasFlagFast(Foo));
            Assert.False(actual.HasFlagFast(Bar));
            Assert.True(actual.HasFlagFast(Baz));
        }

        [Fact]
        void AttributeParserThrowsIfPassedNonEnumType()
        {
            Assert.Throws<ArgumentException>
            (
                () =>
                    DllMapAttributeParser.Parse<int>(NonNegatedAttributeList)
            );
        }

        [Fact]
        void AttributeParserThrowsIfPassedEnumTypeWithoutFlagAttribute()
        {
            Assert.Throws<ArgumentException>
            (
                () =>
                    DllMapAttributeParser.Parse<TestEnumWithoutFlagAttribute>(NonNegatedAttributeList)
            );
        }

        [Fact]
        void AttributeParserReturnsAllPossibleValuesForNullInput()
        {
            var expected = Enum.GetValues(typeof(TestEnum)).Cast<TestEnum>().Aggregate((a, b) => a | b);
            var actual = DllMapAttributeParser.Parse<TestEnum>(null);

            Assert.Equal(expected, actual);
        }
    }
}