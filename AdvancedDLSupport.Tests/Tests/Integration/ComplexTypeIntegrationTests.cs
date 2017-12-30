using AdvancedDLSupport.Tests.Data;
using Xunit;

namespace AdvancedDLSupport.Tests.Integration
{
    public class ComplexTypeIntegrationTests
    {
        private const string LibraryName = "ComplexTypeTests";

        [Fact]
        public void CanCallFunctionWithStringReturnValue()
        {
            var library = new AnonymousImplementationBuilder().ResolveAndActivateInterface<IComplexTypeLibrary>(LibraryName);
            var actual = library.GetString();
            Assert.Equal("Hello from C!", actual);
        }

        [Fact]
        public void CanCallFunctionWithStringReturnValueWhereResultIsNull()
        {
            var library = new AnonymousImplementationBuilder().ResolveAndActivateInterface<IComplexTypeLibrary>(LibraryName);
            Assert.Null(library.GetNullString());
        }

        [Fact]
        public void CanCallFunctionWithStringParameter()
        {
            var library = new AnonymousImplementationBuilder().ResolveAndActivateInterface<IComplexTypeLibrary>(LibraryName);
            const string testString = "I once knew a polish audio engineer";
            var expected = testString.Length;

            Assert.Equal(expected, (long)library.StringLength(testString).ToUInt64());
        }

        [Fact]
        public void CanCallFunctionWithStringParameterWhereParameterIsNull()
        {
            var library = new AnonymousImplementationBuilder().ResolveAndActivateInterface<IComplexTypeLibrary>(LibraryName);

            Assert.True(library.CheckIfStringIsNull(null));
        }

        [Fact]
        public void CanCallFunctionWithNullableReturnValue()
        {
            var library = new AnonymousImplementationBuilder().ResolveAndActivateInterface<IComplexTypeLibrary>(LibraryName);

            var result = library.GetAllocatedTestStruct();
            Assert.NotNull(result);
            Assert.Equal(10, result.Value.A);
            Assert.Equal(20, result.Value.B);
        }
        [Fact]
        public void CanCallFunctionWithNullableReturnValueWhereResultIsNull()
        {
            var library = new AnonymousImplementationBuilder().ResolveAndActivateInterface<IComplexTypeLibrary>(LibraryName);

            var result = library.GetNullTestStruct();
            Assert.Null(result);
        }

        [Fact]
        public void CanCallFunctionWithNullableParameter()
        {
            var library = new AnonymousImplementationBuilder().ResolveAndActivateInterface<IComplexTypeLibrary>(LibraryName);

            Assert.False(library.CheckIfStructIsNull(new TestStruct{ A = 10, B = 20 }));
        }

        [Fact]
        public void CanCallFunctionWithNullableParameterWhereParameterIsNull()
        {
            var library = new AnonymousImplementationBuilder().ResolveAndActivateInterface<IComplexTypeLibrary>(LibraryName);

            Assert.True(library.CheckIfStructIsNull(null));
        }
    }
}
