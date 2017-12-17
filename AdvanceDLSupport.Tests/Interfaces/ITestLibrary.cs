using AdvanceDLSupport.Tests.Structures;

#pragma warning disable SA1600, CS1591 // Elements should be documented

namespace AdvanceDLSupport.Tests.Interfaces
{
    public interface ITestLibrary
    {
        int Multiply(ref TestStruct struc, int multiplier);
        int Multiply(int value, int multiplier);
    }
}
