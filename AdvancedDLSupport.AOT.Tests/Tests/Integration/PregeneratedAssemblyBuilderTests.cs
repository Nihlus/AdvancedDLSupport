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
                Builder.Build(OutputPath);

                Assert.True(File.Exists(OutputPath));
            }

            [Fact]
            public void GeneratesAnOutputFileForASourceExplicitCombination()
            {
                Builder.WithSourceExplicitTypeCombination<AOTMixedModeClass, IAOTLibrary>();
                Builder.Build(OutputPath);

                Assert.True(File.Exists(OutputPath));
            }
        }
    }
}
