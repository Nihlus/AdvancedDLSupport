using System.Runtime.InteropServices;
using AdvancedDLSupport;
using AdvanceDLSupport.Tests.Structures;

namespace AdvanceDLSupport.Tests.Interfaces
{
    public interface IFunctionLibrary
    {
        int DoStructMath(ref TestStruct struc, int multiplier);

        int Multiply(int value, int multiplier);

        [NativeSymbol(nameof(DoStructMath))]
        int Multiply(ref TestStruct value, int multiplier);

        int Subtract(int value, int other);

        [NativeSymbol(nameof(Subtract))]
        int DuplicateSubtract(int value, int other);

        [NativeSymbol(CallingConvention = CallingConvention.StdCall)]
        int STDCALLSubtract(int value, int other);
    }
}
