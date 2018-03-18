using System;
using AdvancedDLSupport.Tests.Data;
using AdvancedDLSupport.Tests.Data.Classes;
using AdvancedDLSupport.Tests.TestBases;
using Xunit;

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