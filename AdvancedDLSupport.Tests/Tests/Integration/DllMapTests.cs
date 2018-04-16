using AdvancedDLSupport.Tests.Data;
using AdvancedDLSupport.Tests.TestBases;
using Xunit;

namespace AdvancedDLSupport.Tests.Integration
{
    public class DllMapTests : LibraryTestBase<IRemappedLibrary>
    {
        private const string LibraryName = "OriginalLibraryName";

        public DllMapTests() : base(LibraryName)
        {
        }

        protected override ImplementationOptions GetImplementationOptions()
        {
            return ImplementationOptions.EnableDllMapSupport;
        }

        [Fact]
        public void RemappedLibraryMapsToCorrectLibrary()
        {
            Assert.Equal(25, Library.Multiply(5, 5));
        }
    }
}