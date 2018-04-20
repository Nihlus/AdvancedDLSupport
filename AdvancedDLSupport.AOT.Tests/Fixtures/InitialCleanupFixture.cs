using System.IO;

namespace AdvancedDLSupport.AOT.Tests.Fixtures
{
    public class InitialCleanupFixture
    {
        public InitialCleanupFixture()
        {
            var targetDirectory = new DirectoryInfo(Path.Combine(Directory.GetCurrentDirectory(), "aot-test"));
            if (targetDirectory.Exists)
            {
                targetDirectory.Delete(true);
            }
        }
    }
}
