using System.IO;
using AdvancedDLSupport.AOT.Tests.Fixtures;
using Xunit;

namespace AdvancedDLSupport.AOT.Tests.Tests.Integration
{
    public class ProgramTests : IClassFixture<InitialCleanupFixture>
    {
        [Fact]
        public void ReturnsInputAssemblyNotFoundIfOneOrMoreAssembliesDoNotExist()
        {
            var args = "--input-assemblies aaaa.dll".Split(' ');

            var result = Program.Main(args);

            Assert.Equal(ExitCodes.InputAssemblyNotFound, (ExitCodes)result);
        }

        [Fact]
        public void ReturnsFailedToLoadAssemblyIfOneOrMoreAssembliesCouldNotBeLoaded()
        {
            File.Create("empty.dll").Close();
            var args = "--input-assemblies empty.dll".Split(' ');

            var result = Program.Main(args);

            File.Delete("empty.dll");
            Assert.Equal(ExitCodes.FailedToLoadAssembly, (ExitCodes)result);
        }

        [Fact]
        public void ReturnsSuccessIfNoErrorsWereGenerated()
        {
            var args = "--input-assemblies AdvancedDLSupport.AOT.Tests.dll -o aot-test".Split(' ');

            var result = Program.Main(args);

            Assert.Equal(ExitCodes.Success, (ExitCodes)result);
        }
    }
}
