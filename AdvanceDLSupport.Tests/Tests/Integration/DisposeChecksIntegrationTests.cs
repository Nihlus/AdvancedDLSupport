using System;
using AdvancedDLSupport;
using AdvanceDLSupport.Tests.Data;
using Xunit;

namespace AdvanceDLSupport.Tests.Integration
{
    public class DisposeChecksIntegrationTests
    {
        private const string LibraryName = "DisposeTests";

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
            var config = new ImplementationConfiguration(generateDisposalChecks:true);
            var library = new AnonymousImplementationBuilder(config).ResolveAndActivateInterface<IDisposeCheckLibrary>(LibraryName);

            library.Multiply(5, 5);
        }

        [Fact]
        public void DisposedLibraryThrows()
        {
            var config = new ImplementationConfiguration(generateDisposalChecks:true);
            var library = new AnonymousImplementationBuilder(config).ResolveAndActivateInterface<IDisposeCheckLibrary>(LibraryName);
            library.Dispose();

            Assert.Throws<ObjectDisposedException>(() => library.Multiply(5, 5));
        }

        [Fact]
        public void CanGetNewInstanceOfInterfaceAfterDisposalOfExistingInstance()
        {
            var config = new ImplementationConfiguration(generateDisposalChecks:true);
            var library = new AnonymousImplementationBuilder(config).ResolveAndActivateInterface<IDisposeCheckLibrary>(LibraryName);
            library.Dispose();

            var newLibrary = new AnonymousImplementationBuilder(config).ResolveAndActivateInterface<IDisposeCheckLibrary>(LibraryName);

            newLibrary.Multiply(5, 5);
            Assert.NotSame(library, newLibrary);
        }
    }
}
