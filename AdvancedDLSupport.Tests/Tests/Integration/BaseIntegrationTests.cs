using AdvancedDLSupport.Tests.Data;
using AdvancedDLSupport.Tests.TestBases;
using Xunit;

namespace AdvancedDLSupport.Tests.Integration
{
    public class BaseIntegrationTests : LibraryTestBase<IBaseLibrary>
    {
        private const string LibraryName = "BaseTests";

        public BaseIntegrationTests() : base(LibraryName)
        {
        }

        [Fact]
        public void CanLoadLibrary()
        {
            Assert.NotNull(_library);
        }

        [Fact]
        public void LoadingSameInterfaceAndSameFileTwiceProducesDifferentReferences()
        {
            var secondLoad = new AnonymousImplementationBuilder().ResolveAndActivateInterface<IBaseLibrary>(LibraryName);

            Assert.NotSame(_library, secondLoad);
        }

        [Fact]
        public void LoadingSameInterfaceAndSameFileTwiceUsesSameGeneratedType()
        {
            var secondLoad = new AnonymousImplementationBuilder().ResolveAndActivateInterface<IBaseLibrary>(LibraryName);

            Assert.IsType(_library.GetType(), secondLoad);
        }

        [Fact]
        public void LoadingSameInterfaceAndSameFileButWithDifferentOptionsDoesNotUseSameGeneratedType()
        {
            var options = ImplementationOptions.UseLazyBinding;

            var secondLoad = new AnonymousImplementationBuilder(options).ResolveAndActivateInterface<IBaseLibrary>(LibraryName);

            Assert.IsNotType(_library.GetType(), secondLoad);
        }

        [Fact]
        public void LoadingSameInterfaceAndSameFileButWithDifferentOptionsProducesDifferentReferences()
        {
            var options = ImplementationOptions.UseLazyBinding;

            var secondLoad = new AnonymousImplementationBuilder(options).ResolveAndActivateInterface<IBaseLibrary>(LibraryName);

            Assert.NotSame(_library, secondLoad);
        }
    }
}