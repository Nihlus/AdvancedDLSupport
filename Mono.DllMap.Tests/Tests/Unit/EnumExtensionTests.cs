//
//  EnumExtensionTests.cs
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

using System;
using System.Linq;
using Mono.DllMap.Extensions;
using Mono.DllMap.Tests.Data;
using Xunit;

#pragma warning disable SA1600, CS1591

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
                        default(int).HasFlagFast(default(int))
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
                        default(int).HasFlagsFast(default(int), default(int))
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
                        default(int).HasAll()
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
                        default(int).GetFlags().ToList()
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
