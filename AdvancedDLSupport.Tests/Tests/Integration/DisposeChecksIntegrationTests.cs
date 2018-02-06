using System;
using AdvancedDLSupport.Tests.Data;
using AdvancedDLSupport.Tests.TestBases;
using Xunit;

namespace AdvancedDLSupport.Tests.Integration
{
    public class DisposeChecksIntegrationTests : LibraryTestBase<IDisposeCheckLibrary>
    {
        private const string LibraryName = "DisposeTests";

        public DisposeChecksIntegrationTests() : base(LibraryName)
        {
        }

        protected override ImplementationOptions GetImplementationOptions()
        {
            return ImplementationOptions.GenerateDisposalChecks;
        }

        [Fact]
        public void DisposedLibraryWithoutGeneratedChecksDoesNotThrow()
        {
            var library = new AnonymousImplementationBuilder().ResolveAndActivateInterface<IDisposeCheckLibrary>(LibraryName);
            library.Dispose();
            library.Multiply(5, 5);
        }

        [Fact]
        public void UndisposedLibraryDoesNotThrow()
        {
            Library.Multiply(5, 5);
        }

        [Fact]
        public void DisposedLibraryThrows()
        {
            Library.Dispose();

            Assert.Throws<ObjectDisposedException>(() => Library.Multiply(5, 5));
        }

        [Fact]
        public void CanGetNewInstanceOfInterfaceAfterDisposalOfExistingInstance()
        {
            Library.Dispose();

            var newLibrary = new AnonymousImplementationBuilder(Config).ResolveAndActivateInterface<IDisposeCheckLibrary>(LibraryName);

            newLibrary.Multiply(5, 5);
            Assert.NotSame(Library, newLibrary);
        }
    }
}
