using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;

namespace AdvancedDLSupport.AOT.Tests.TestBases
{
    public class PregeneratedAssemblyBuilderTestBase : IDisposable
    {
        private const string OutputBaseName = "DLSupportDynamicAssembly";

        protected string OutputDirectory { get; }

        protected Assembly SourceAssembly { get; }
        protected PregeneratedAssemblyBuilder Builder { get; }

        [SuppressMessage("ReSharper", "VirtualMemberCallInConstructor")]
        protected PregeneratedAssemblyBuilderTestBase()
        {
            SourceAssembly = Assembly.GetAssembly(typeof(PregeneratedAssemblyBuilderTestBase));
            Builder = new PregeneratedAssemblyBuilder(GetImplementationOptions());

            OutputDirectory = Path.Combine(Directory.GetCurrentDirectory(), "aot-test");
        }

        protected virtual ImplementationOptions GetImplementationOptions()
        {
            return 0;
        }

        public virtual void Dispose()
        {
            var generatedAssemblies = Directory.EnumerateFiles(OutputDirectory, $"{OutputBaseName}*");

            foreach (var assembly in generatedAssemblies)
            {
                 File.Delete(assembly);
            }

            Directory.Delete(OutputDirectory, true);
        }
    }
}
