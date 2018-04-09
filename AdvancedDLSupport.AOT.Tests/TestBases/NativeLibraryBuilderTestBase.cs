using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace AdvancedDLSupport.AOT.Tests.TestBases
{
    public class NativeLibraryBuilderTestBase : PregeneratedAssemblyBuilderTestBase
    {
        protected string AOTDirectory { get; }

        protected NativeLibraryBuilder LibraryBuilder { get; }

        [SuppressMessage("ReSharper", "VirtualMemberCallInConstructor")]
        protected NativeLibraryBuilderTestBase()
        {
            LibraryBuilder = new NativeLibraryBuilder(GetImplementationOptions());
            AOTDirectory = Path.Combine(Directory.GetCurrentDirectory(), "aot");
        }

        protected override ImplementationOptions GetImplementationOptions()
        {
            return ImplementationOptions.UseLazyBinding;
        }

        public override void Dispose()
        {
            base.Dispose();

            Directory.Delete(AOTDirectory, true);
        }
    }
}