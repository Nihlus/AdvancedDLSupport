using System.Runtime.InteropServices;
using AdvancedDLSupport.Attributes;
using AdvanceDLSupport.Tests.Structures;

#pragma warning disable SA1600, CS1591 // Elements should be documented

namespace AdvanceDLSupport.Tests.Interfaces
{
    public unsafe interface ITestLibrary
    {
        int GlobalVariableA { get; set; }
        int* GlobalPointerVariable { get; set; }

        void InitializeGlobalPointerVariable();
        int DoStructMath(ref TestStruct struc, int multiplier);

        [NativeFunction("DoStructMath")]
        int Multiply(ref TestStruct struc, int multiplier);

        int Multiply(int value, int multiplier);

        [NativeFunction(CallingConvention = CallingConvention.Cdecl)]
        int CDeclSubtract(int value, int other);

        [NativeFunction("CDeclSubtract", CallingConvention = CallingConvention.Cdecl)]
        int Subtract(int value, int other);
    }
}
