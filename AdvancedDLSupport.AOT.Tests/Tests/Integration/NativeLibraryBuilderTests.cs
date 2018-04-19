using System.IO;
using System.Reflection;
using AdvancedDLSupport.AOT.Tests.Data.Interfaces;
using AdvancedDLSupport.AOT.Tests.TestBases;
using Xunit;

namespace AdvancedDLSupport.AOT.Tests.Tests.Integration
{
    public class NativeLibraryBuilderTests : NativeLibraryBuilderTestBase
    {
        public class DiscoverCompiledTypes : NativeLibraryBuilderTestBase
        {
            [Fact]
            public void CanDiscoverPrecompiledTypes()
            {
                // Pregenerate the types
                Builder.WithSourceAssembly(GetType().Assembly);
                Builder.Build(OutputDirectory);

                NativeLibraryBuilder.DiscoverCompiledTypes(OutputDirectory);
            }
        }

        [Fact]
        public void UsesPrecompiledTypesIfDiscovered()
        {
            // Pregenerate the types
            Builder.WithSourceAssembly(GetType().Assembly);
            Builder.Build(OutputDirectory);

            NativeLibraryBuilder.DiscoverCompiledTypes(OutputDirectory);

            var library = LibraryBuilder.ActivateInterface<IAOTLibrary>("AOTTests");

            var libraryAssembly = library.GetType().Assembly;

            Assert.False(libraryAssembly.GetCustomAttribute<AOTAssemblyAttribute>() is null);
        }
    }
}