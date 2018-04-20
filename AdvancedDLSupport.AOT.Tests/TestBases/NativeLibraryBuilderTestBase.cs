using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace AdvancedDLSupport.AOT.Tests.TestBases
{
    public class NativeLibraryBuilderTestBase : PregeneratedAssemblyBuilderTestBase
    {
        protected NativeLibraryBuilder LibraryBuilder { get; }

        [SuppressMessage("ReSharper", "VirtualMemberCallInConstructor")]
        protected NativeLibraryBuilderTestBase()
        {
            LibraryBuilder = new NativeLibraryBuilder(GetImplementationOptions());
        }

        protected override ImplementationOptions GetImplementationOptions()
        {
            return ImplementationOptions.UseLazyBinding;
        }
    }
}