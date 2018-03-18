using System;
using System.Linq;
using Mono.DllMap.Extensions;
using Mono.DllMap.Tests.Data;
using Xunit;

namespace Mono.DllMap.Tests.Unit
{
    public class EnumExtensionTests
    {
        public class HasFlagFast
        {
            [Fact]
            public void ReturnsTrueIfValueHasFlag()
            {
                var value = TestEnum.Foo | TestEnum.Bar;

                Assert.True(value.HasFlagFast(TestEnum.Foo));
                Assert.True(value.HasFlagFast(TestEnum.Bar));
            }

            [Fact]
            public void ReturnsFaseIfValueDoesNotHaveFlag()
            {
                var value = TestEnum.Foo | TestEnum.Bar;

                Assert.False(value.HasFlagFast(TestEnum.Baz));
            }

            [Fact]
            public void ThrowsIfTypeArgumentIsNotAnEnum()
            {
                Assert.Throws<ArgumentException>
                (
                    () =>
                        new int().HasFlagFast(new int())
                );
            }

             [Fact]
            public void ThrowsIfTypeArgumentIsNotAFlagsEnum()
            {
                Assert.Throws<ArgumentException>
                (
                    () =>
                        TestEnumWithoutFlagAttribute.Foo.HasFlagFast(TestEnumWithoutFlagAttribute.Bar)
                );
            }
        }

        public class HasFlagsFast
        {
            [Fact]
            public void ReturnsTrueIfValueHasFlags()
            {
                var value = TestEnum.Foo | TestEnum.Bar;

                Assert.True(value.HasFlagsFast(TestEnum.Foo, TestEnum.Bar));
            }

            [Fact]
            public void ReturnsFaseIfValueDoesNotHaveFlags()
            {
                var value = TestEnum.Foo;

                Assert.False(value.HasFlagsFast(TestEnum.Bar, TestEnum.Baz));
            }

            [Fact]
            public void ThrowsIfTypeArgumentIsNotAnEnum()
            {
                Assert.Throws<ArgumentException>
                (
                    () =>
                        new int().HasFlagsFast(new int(), new int())
                );
            }

            [Fact]
            public void ThrowsIfTypeArgumentIsNotAFlagsEnum()
            {
                Assert.Throws<ArgumentException>
                (
                    () =>
                        TestEnumWithoutFlagAttribute.Foo.HasFlagsFast(TestEnumWithoutFlagAttribute.Bar, TestEnumWithoutFlagAttribute.Baz)
                );
            }
        }

        public class HasAll
        {
            [Fact]
            public void ReturnsTrueIfValueHasAllFlags()
            {
                var value = TestEnum.Foo | TestEnum.Bar | TestEnum.Baz;

                Assert.True(value.HasAll());
            }

            [Fact]
            public void ReturnsFaseIfValueDoesNotHaveAllFlags()
            {
                var value = TestEnum.Foo;

                Assert.False(value.HasAll());
            }

            [Fact]
            public void ThrowsIfTypeArgumentIsNotAnEnum()
            {
                Assert.Throws<ArgumentException>
                (
                    () =>
                        new int().HasAll()
                );
            }

            [Fact]
            public void ThrowsIfTypeArgumentIsNotAFlagsEnum()
            {
                Assert.Throws<ArgumentException>
                (
                    () =>
                        TestEnumWithoutFlagAttribute.Foo.HasAll()
                );
            }
        }

        public class GetFlags
        {
            [Fact]
            public void ReturnsASetOfTheFlagsInTheValue()
            {
                var value = TestEnum.Foo | TestEnum.Bar | TestEnum.Baz;

                var actual = value.GetFlags();

                Assert.Equal(new[] { TestEnum.Foo, TestEnum.Bar, TestEnum.Baz }, actual);
            }

            [Fact]
            public void ThrowsIfTypeArgumentIsNotAnEnum()
            {
                Assert.Throws<ArgumentException>
                (
                    () =>
                        new int().GetFlags().ToList()
                );
            }

            [Fact]
            public void ThrowsIfTypeArgumentIsNotAFlagsEnum()
            {
                Assert.Throws<ArgumentException>
                (
                    () =>
                        TestEnumWithoutFlagAttribute.Foo.GetFlags().ToList()
                );
            }
        }
    }
}
