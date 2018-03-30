using System;
using System.IO;
using System.Reflection;

namespace AdvancedDLSupport.AOT.Tests.TestBases
{
    public class PregeneratedAssemblyBuilderTestBase : IDisposable
    {
        protected const string OutputPath = "tests_output.dll";

        protected Assembly SourceAssembly { get; }
        protected PregeneratedAssemblyBuilder Builder { get; }

        protected PregeneratedAssemblyBuilderTestBase()
        {
            SourceAssembly = Assembly.GetAssembly(typeof(PregeneratedAssemblyBuilderTestBase));
            Builder = new PregeneratedAssemblyBuilder();
        }

        public void Dispose()
        {
            if (File.Exists(OutputPath))
            {
                File.Delete(OutputPath);
            }
        }
    }
}
