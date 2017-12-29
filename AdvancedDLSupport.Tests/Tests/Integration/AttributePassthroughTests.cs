using AdvancedDLSupport.Tests.Data;
using Xunit;

namespace AdvancedDLSupport.Tests.Integration
{
    public class AttributePassthroughTests
    {
        private const string LibraryName = nameof(AttributePassthroughTests);

        [Fact]
        public void RespectsReturnValueMarshalAs()
        {
            var library = new AnonymousImplementationBuilder().ResolveAndActivateInterface<IAttributePassthroughLibrary>(LibraryName);

            Assert.True(library.CheckIfGreaterThanZero(1));
            Assert.False(library.CheckIfGreaterThanZero(0));
            Assert.True(library.CheckIfStringIsNull(null));
            Assert.False(library.CheckIfStringIsNull("Hello!"));
        }

        [Fact]
        public void RespectsStringMarshallingAttribute()
        {
            var library = new AnonymousImplementationBuilder().ResolveAndActivateInterface<IAttributePassthroughLibrary>(LibraryName);
            var expected = "Äta gurka i en båt på ön";

            Assert.Equal(expected, library.EchoWString(expected));
        }
    }
}
