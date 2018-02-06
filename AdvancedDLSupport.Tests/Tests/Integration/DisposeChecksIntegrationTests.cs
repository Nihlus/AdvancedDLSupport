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
            _library.Multiply(5, 5);
        }

        [Fact]
        public void DisposedLibraryThrows()
        {
            _library.Dispose();

            Assert.Throws<ObjectDisposedException>(() => _library.Multiply(5, 5));
        }

        [Fact]
        public void CanGetNewInstanceOfInterfaceAfterDisposalOfExistingInstance()
        {
            _library.Dispose();

            var newLibrary = new AnonymousImplementationBuilder(_config).ResolveAndActivateInterface<IDisposeCheckLibrary>(LibraryName);

            newLibrary.Multiply(5, 5);
            Assert.NotSame(_library, newLibrary);
        }
    }
}
