using System;
using System.Diagnostics.CodeAnalysis;
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
            Builder = new PregeneratedAssemblyBuilder(GetImplementationOptions());
        }

        protected virtual ImplementationOptions GetImplementationOptions()
        {
            return 0;
        }

        public virtual void Dispose()
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
