using System;
using System.IO;
using System.Reflection;

namespace AdvancedDLSupport.AOT.Tests.TestBases
{
    public class PregeneratedAssemblyBuilderTestBase : IDisposable
    {
        protected const string OutputName = "DLSupportDynamicAssembly.dll";

        protected Assembly SourceAssembly { get; }
        protected PregeneratedAssemblyBuilder Builder { get; }

        protected PregeneratedAssemblyBuilderTestBase()
        {
            SourceAssembly = Assembly.GetAssembly(typeof(PregeneratedAssemblyBuilderTestBase));
            Builder = new PregeneratedAssemblyBuilder();
        }

        public void Dispose()
        {
            var currentDirectory = Directory.GetCurrentDirectory();
            var generatedAssemblies = Directory.EnumerateFiles(currentDirectory, $"{Path.GetFileNameWithoutExtension(OutputName)}*");

            foreach (var assembly in generatedAssemblies)
            {
                 File.Delete(assembly);
            }
        }
    }
}
