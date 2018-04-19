using System.IO;
using AdvancedDLSupport.AOT.Tests.Data.Classes;
using AdvancedDLSupport.AOT.Tests.Data.Interfaces;
using AdvancedDLSupport.AOT.Tests.TestBases;
using Xunit;

namespace AdvancedDLSupport.AOT.Tests.Tests.Integration
{
    public class PregeneratedAssemblyBuilderTests
    {
        public class Build : PregeneratedAssemblyBuilderTestBase
        {
            [Fact]
            public void GeneratesAnOutputFileForASourceAssembly()
            {
                Builder.WithSourceAssembly(SourceAssembly);
                var result = Builder.Build(OutputDirectory);

                var outputFile = Path.Combine(OutputDirectory, result);
                Assert.True(File.Exists(outputFile));
            }

            [Fact]
            public void GeneratesAnOutputFileForASourceExplicitCombination()
            {
                Builder.WithSourceExplicitTypeCombination<AOTMixedModeClass, IAOTLibrary>();
                var result = Builder.Build(OutputDirectory);

                var outputFile = Path.Combine(OutputDirectory, result);
                Assert.True(File.Exists(outputFile));
            }
        }
    }
}
