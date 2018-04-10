using System.IO;
using Xunit;

namespace AdvancedDLSupport.AOT.Tests.Tests.Integration
{
    public class ProgramTests
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
            File.Create("empty.dll");
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

            Directory.Delete("aot-test", true);
            Assert.Equal(ExitCodes.Success, (ExitCodes)result);
        }
    }
}
