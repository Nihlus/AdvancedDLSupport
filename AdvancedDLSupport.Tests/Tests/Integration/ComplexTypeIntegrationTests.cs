using AdvancedDLSupport.Tests.Data;
using AdvancedDLSupport.Tests.TestBases;
using Xunit;

namespace AdvancedDLSupport.Tests.Integration
{
    public class ComplexTypeIntegrationTests : LibraryTestBase<IComplexTypeLibrary>
    {
        private const string LibraryName = "ComplexTypeTests";

        public ComplexTypeIntegrationTests() : base(LibraryName)
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
            Assert.False(Library.CheckIfStructIsNull(new TestStruct{ A = 10, B = 20 }));
        }

        [Fact]
        public void CanCallFunctionWithNullableParameterWhereParameterIsNull()
        {
            Assert.True(Library.CheckIfStructIsNull(null));
        }
    }
}
