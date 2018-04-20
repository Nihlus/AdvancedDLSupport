using System;
using System.Diagnostics.CodeAnalysis;

namespace AdvancedDLSupport.Tests.TestBases
{
    public abstract class LibraryTestBase<T> : IDisposable where T : class
    {
        protected readonly ImplementationOptions Config;
        protected readonly T Library;

        protected readonly NativeLibraryBuilder Builder;

        [SuppressMessage("ReSharper", "VirtualMemberCallInConstructor")]
        public LibraryTestBase(string libraryLocation)
        {
            Config = GetImplementationOptions();

            Builder = new NativeLibraryBuilder(Config);
            Library = Builder.ActivateInterface<T>(libraryLocation);
        }

        protected virtual ImplementationOptions GetImplementationOptions()
        {
            return ImplementationOptions.GenerateDisposalChecks;
        }

        public void Dispose()
        {
            var libraryBase = Library as NativeLibraryBase;
            libraryBase?.Dispose();
        }
    }
}