using AdvancedDLSupport.Tests.Data;
using AdvancedDLSupport.Tests.TestBases;
using Xunit;

namespace AdvancedDLSupport.Tests.Integration
{
    public class AttributePassthroughTests : LibraryTestBase<IAttributePassthroughLibrary>
    {
        private const string LibraryName = nameof(AttributePassthroughTests);

        public AttributePassthroughTests() : base(LibraryName)
        {
        }

        [Fact]
        public void RespectsReturnValueMarshalAs()
        {
            Assert.True(Library.CheckIfGreaterThanZero(1));
            Assert.False(Library.CheckIfGreaterThanZero(0));
            Assert.True(Library.CheckIfStringIsNull(null));
            Assert.False(Library.CheckIfStringIsNull("Hello!"));
        }

        [Fact]
        public void RespectsStringMarshallingAttribute()
        {
            var expected = "Äta gurka i en båt på ön";

            Assert.Equal(expected, Library.EchoWString(expected));
        }
    }
}
