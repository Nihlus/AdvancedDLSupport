using AdvancedDLSupport.Tests.Data;
using AdvancedDLSupport.Tests.TestBases;
using Xunit;

namespace AdvancedDLSupport.Tests.Integration
{
    public class TypeLoweringIntegrationTests
    {
        public class StringTransformerTests : LibraryTestBase<ITypeLoweringLibrary>
        {
            private const string LibraryName = "TypeLoweringTests";

            public StringTransformerTests() : base(LibraryName)
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
            public void CanCallFunctionWithBStrReturnValue()
            {
                var actual = Library.EchoBStr("Hello from C!");
                Assert.Equal("Hello from C!", actual);
            }

            [Fact]
            public void CanCallFunctionWithLPTStrReturnValue()
            {
                var actual = Library.GetLPTString();
                Assert.Equal("Hello from C!", actual);
            }

            [Fact]
            public void CanCallFunctionWithLPWStrReturnValue()
            {
                var actual = Library.GetLPWString();
                Assert.Equal("Hello from C!", actual);
            }

            [Fact]
            public void CanCallFunctionWithBStrParameter()
            {
                const string testString = "I once knew a polish audio engineer";
                var expected = testString.Length;

                Assert.Equal(expected, (long)Library.BStringLength(testString).ToUInt64());
            }

            [Fact]
            public void CanCallFunctionWithLPTStrParameter()
            {
                const string testString = "I once knew a polish audio engineer";
                var expected = testString.Length;

                Assert.Equal(expected, (long)Library.LPTStringLength(testString).ToUInt64());
            }

            [Fact]
            public void CanCallFunctionWithLPWStrParameter()
            {
                const string testString = "I once knew a polish audio engineer";
                var expected = testString.Length;

                Assert.Equal(expected, (long)Library.LPWStringLength(testString).ToUInt64());
            }
        }
    }
}
