using AdvancedDLSupport.Tests.Data;
using Xunit;

namespace AdvancedDLSupport.Tests.Integration
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
        public void LoadingSameInterfaceAndSameFileTwiceProducesDifferentReferences()
        {
            var firstLoad = new AnonymousImplementationBuilder().ResolveAndActivateInterface<IBaseLibrary>(LibraryName);
            var secondLoad = new AnonymousImplementationBuilder().ResolveAndActivateInterface<IBaseLibrary>(LibraryName);

            Assert.NotSame(firstLoad, secondLoad);
        }

        [Fact]
        public void LoadingSameInterfaceAndSameFileTwiceUsesSameGeneratedType()
        {
            var firstLoad = new AnonymousImplementationBuilder().ResolveAndActivateInterface<IBaseLibrary>(LibraryName);
            var secondLoad = new AnonymousImplementationBuilder().ResolveAndActivateInterface<IBaseLibrary>(LibraryName);

            Assert.IsType(firstLoad.GetType(), secondLoad);
        }

        [Fact]
        public void LoadingSameInterfaceAndSameFileButWithDifferentOptionsDoesNotUseSameGeneratedType()
        {
            var options = new ImplementationConfiguration()
            {
                UseLazyBinding = true
            };

            var firstLoad = new AnonymousImplementationBuilder(options).ResolveAndActivateInterface<IBaseLibrary>(LibraryName);

            var secondLoad = new AnonymousImplementationBuilder().ResolveAndActivateInterface<IBaseLibrary>(LibraryName);

            Assert.IsNotType(firstLoad.GetType(), secondLoad);
        }

        [Fact]
        public void LoadingSameInterfaceAndSameFileButWithDifferentOptionsProducesDifferentReferences()
        {
            var options = new ImplementationConfiguration()
            {
                UseLazyBinding = true
            };

            var firstLoad = new AnonymousImplementationBuilder(options).ResolveAndActivateInterface<IBaseLibrary>(LibraryName);

            var secondLoad = new AnonymousImplementationBuilder().ResolveAndActivateInterface<IBaseLibrary>(LibraryName);

            Assert.NotSame(firstLoad, secondLoad);
        }
    }
}