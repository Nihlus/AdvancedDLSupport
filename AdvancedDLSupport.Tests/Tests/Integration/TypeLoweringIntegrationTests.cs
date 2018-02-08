using AdvancedDLSupport.Tests.Data;
using AdvancedDLSupport.Tests.TestBases;
using Xunit;

namespace AdvancedDLSupport.Tests.Integration
{
    public class TypeLoweringIntegrationTests : LibraryTestBase<ITypeLoweringLibrary>
    {
        private const string LibraryName = "TypeLoweringTests";

        public TypeLoweringIntegrationTests() : base(LibraryName)
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
    }
}
