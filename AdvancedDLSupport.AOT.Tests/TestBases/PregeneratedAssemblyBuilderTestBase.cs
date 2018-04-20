using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using AdvancedDLSupport.AOT.Tests.Fixtures;
using Xunit;

namespace AdvancedDLSupport.AOT.Tests.TestBases
{
    public class PregeneratedAssemblyBuilderTestBase : IClassFixture<InitialCleanupFixture>
    {
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
    }
}
