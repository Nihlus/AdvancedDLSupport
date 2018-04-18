using static System.Runtime.InteropServices.CallingConvention;

namespace AdvancedDLSupport.Tests.Data
{
    public interface INameManglingTests
    {
        [NativeSymbol(CallingConvention = StdCall)]
        int Multiply(int a, int b);

        [NativeSymbol(CallingConvention = StdCall)]
        int MultiplyStructByVal(TestStruct strct);

        [NativeSymbol(Entrypoint = nameof(MultiplyStructByPtr), CallingConvention = StdCall)]
        int MultiplyStructByRef(ref TestStruct strct);

        [NativeSymbol(CallingConvention = StdCall)]
        unsafe int MultiplyStructByPtr(TestStruct* strct);
    }
}
