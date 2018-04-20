using System.IO;
using System.Linq;
using System.Reflection;
using AdvancedDLSupport.AOT.Tests.Data.Interfaces;
using AdvancedDLSupport.AOT.Tests.TestBases;
using Xunit;

namespace AdvancedDLSupport.AOT.Tests.Tests.Integration
{
    public class NativeLibraryBuilderTests
    {
        public class DiscoverCompiledTypes : NativeLibraryBuilderTestBase
        {
            [Fact]
            public void CanDiscoverPrecompiledTypes()
            {
                // Pregenerate the types
                Builder.WithSourceAssembly(GetType().Assembly);
                var result = Builder.Build(OutputDirectory);

                var searchPattern = $"*{new string(result.SkipWhile(c => c == '_').TakeWhile(c => c != '_').ToArray())}*.dll";
                LibraryBuilder.DiscoverCompiledTypes(OutputDirectory, searchPattern);
            }

            [Fact]
            public void UsesPrecompiledTypesIfDiscovered()
            {
                // Pregenerate the types
                Builder.WithSourceAssembly(GetType().Assembly);
                var result = Builder.Build(OutputDirectory);

                var searchPattern = $"*{new string(result.SkipWhile(c => c == '_').TakeWhile(c => c != '_').ToArray())}*.dll";
                LibraryBuilder.DiscoverCompiledTypes(OutputDirectory, searchPattern);

                var library = LibraryBuilder.ActivateInterface<IAOTLibrary>("AOTTests");

                var libraryAssembly = library.GetType().Assembly;

                Assert.False(libraryAssembly.GetCustomAttribute<AOTAssemblyAttribute>() is null);
            }

            [Fact]
            public void SkipsAlreadyGeneratedTypes()
            {
                // Activate the library before scanning for compiled types
                LibraryBuilder.ActivateInterface<IAOTLibrary>("AOTTests");

                // Pregenerate the types
                Builder.WithSourceAssembly(GetType().Assembly);
                var result = Builder.Build(OutputDirectory);

                var searchPattern = $"*{new string(result.SkipWhile(c => c == '_').TakeWhile(c => c != '_').ToArray())}*.dll";
                LibraryBuilder.DiscoverCompiledTypes(OutputDirectory, searchPattern);

                var library = LibraryBuilder.ActivateInterface<IAOTLibrary>("AOTTests");
                var libraryAssembly = library.GetType().Assembly;

                Assert.True(libraryAssembly.GetCustomAttribute<AOTAssemblyAttribute>() is null);
            }
        }
    }
}