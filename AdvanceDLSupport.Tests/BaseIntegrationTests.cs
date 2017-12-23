using AdvancedDLSupport;
using AdvanceDLSupport.Tests.Interfaces;
using Xunit;

namespace AdvanceDLSupport.Tests
{
    public class BaseIntegrationTests
    {
        private const string LibraryName = "BaseTests";

        [Fact]
        public void CanLoadLibrary()
        {
            var library = new AnonymousImplementationBuilder().ResolveAndActivateInterface<IBaseLibrary>(LibraryName);
            Assert.NotNull(library);
        }

        [Fact]
        public void LoadingSameInterfaceAndSameFileTwiceProducesIdenticalReferences()
        {
            var firstLoad = new AnonymousImplementationBuilder().ResolveAndActivateInterface<IBaseLibrary>(LibraryName);
            var secondLoad = new AnonymousImplementationBuilder().ResolveAndActivateInterface<IBaseLibrary>(LibraryName);

            Assert.Same(firstLoad, secondLoad);
        }

        [Fact]
        public void LoadingSameInterfaceAndSameFileButWithDifferentOptionsProducesDifferentReferences()
        {
            var options = new ImplementationConfiguration(true);
            var firstLoad = new AnonymousImplementationBuilder(options).ResolveAndActivateInterface<IBaseLibrary>(LibraryName);

            var secondLoad = new AnonymousImplementationBuilder().ResolveAndActivateInterface<IBaseLibrary>(LibraryName);

            Assert.NotSame(firstLoad, secondLoad);
        }
    }
}