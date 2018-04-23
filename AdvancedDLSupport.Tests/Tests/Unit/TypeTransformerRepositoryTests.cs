//
//  TypeTransformerRepositoryTests.cs
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
using AdvancedDLSupport.Tests.Data.Classes;
using AdvancedDLSupport.Tests.TestBases;
using Xunit;

#pragma warning disable SA1600, CS1591

namespace AdvancedDLSupport.Tests.Unit
{
    public class TypeTransformerRepositoryTests
    {
        public class WithTypeTransformer : TypeTransformerRepositoryTestBase
        {
            [Fact]
            public void AddsInputTransformerToRepository()
            {
                Assert.Throws<NotSupportedException>
                (
                    () => Repository.GetTypeTransformer(typeof(SimpleClass))
                );

                Repository.WithTypeTransformer(typeof(SimpleClass), new SimpleClassTypeTransformer());

                var result = Repository.GetTypeTransformer(typeof(SimpleClass));

                Assert.NotNull(result);
                Assert.IsType<SimpleClassTypeTransformer>(result);
            }
        }

        public class GetTypeTransformer : TypeTransformerRepositoryTestBase
        {
            [Fact]
            public void ThrowsIfRepositoryDoesNotHaveType()
            {
                Assert.Throws<NotSupportedException>
                (
                    () => Repository.GetTypeTransformer(typeof(SimpleClass))
                );
            }

            [Fact]
            public void DoesNotThrowForGettingBuiltInTypesWithoutFirstAddingThem()
            {
                var result = Repository.GetTypeTransformer(typeof(string));

                Assert.NotNull(result);

                result = Repository.GetTypeTransformer(typeof(TestStruct?));

                Assert.NotNull(result);
            }

            [Fact]
            public void ReturnsCorrectTransformerForSimpleTypes()
            {
                Repository.WithTypeTransformer(typeof(SimpleClass), new SimpleClassTypeTransformer());
                var result = Repository.GetTypeTransformer(typeof(SimpleClass));

                Assert.IsType<SimpleClassTypeTransformer>(result);
            }
        }
    }
}
