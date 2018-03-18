using System;
using System.Diagnostics.CodeAnalysis;

namespace AdvancedDLSupport.Tests.TestBases
{
    public abstract class LibraryTestBase<T> : IDisposable where T : class
    {
        protected readonly ImplementationOptions Config;
        protected readonly T Library;

        [SuppressMessage("ReSharper", "VirtualMemberCallInConstructor")]
        public LibraryTestBase(string libraryLocation)
        {
            Config = GetImplementationOptions();
            Library = GetImplementationBuilder().ActivateInterface<T>(libraryLocation);
        }

        protected virtual ImplementationOptions GetImplementationOptions()
        {
            return ImplementationOptions.GenerateDisposalChecks;
        }

        protected virtual NativeLibraryBuilder GetImplementationBuilder()
        {
            return new NativeLibraryBuilder(Config);
        }

        public void Dispose()
        {
            var libraryBase = Library as NativeLibraryBase;
            libraryBase?.Dispose();
        }
    }
}