using AdvancedDLSupport.Tests.Data;
using Xunit;

namespace AdvancedDLSupport.Tests.Integration
{
    public class DllMapTests
    {
        private const string LibraryName = "OriginalLibraryName";

        [Fact]
        void RemappedLibraryMapsToCorrectLibrary()
        {
            var config = new ImplementationConfiguration(enableDllMapSupport: true);
            var library = new AnonymousImplementationBuilder(config).ResolveAndActivateInterface<IRemappedLibrary>(LibraryName);

            Assert.Equal(25, library.Multiply(5, 5));
        }
    }
}