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
            Assert.NotNull(Library);
        }

        [Fact]
        public void LoadingSameInterfaceAndSameFileTwiceProducesDifferentReferences()
        {
            var secondLoad = new NativeLibraryBuilder().ActivateInterface<IBaseLibrary>(LibraryName);

            Assert.NotSame(Library, secondLoad);
        }

        [Fact]
        public void LoadingSameInterfaceAndSameFileTwiceUsesSameGeneratedType()
        {
            var secondLoad = new NativeLibraryBuilder().ActivateInterface<IBaseLibrary>(LibraryName);

            Assert.IsType(Library.GetType(), secondLoad);
        }

        [Fact]
        public void LoadingSameInterfaceAndSameFileButWithDifferentOptionsDoesNotUseSameGeneratedType()
        {
            var options = ImplementationOptions.UseLazyBinding;

            var secondLoad = new NativeLibraryBuilder(options).ActivateInterface<IBaseLibrary>(LibraryName);

            Assert.IsNotType(Library.GetType(), secondLoad);
        }

        [Fact]
        public void LoadingSameInterfaceAndSameFileButWithDifferentOptionsProducesDifferentReferences()
        {
            var options = ImplementationOptions.UseLazyBinding;

            var secondLoad = new NativeLibraryBuilder(options).ActivateInterface<IBaseLibrary>(LibraryName);

            Assert.NotSame(Library, secondLoad);
        }
    }
}