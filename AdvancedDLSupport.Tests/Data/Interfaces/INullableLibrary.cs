using System.Runtime.InteropServices;

namespace AdvancedDLSupport.Tests.Data
{
    public interface INullableLibrary
    {
        TestStruct? GetAllocatedTestStruct();
        TestStruct? GetNullTestStruct();

        [return: MarshalAs(UnmanagedType.I1)]
        bool CheckIfStructIsNull(TestStruct? testStruct);

        long GetStructPtrValue(ref TestStruct? testStruct);

        [return: MarshalAs(UnmanagedType.I1)]
        [NativeSymbol(nameof(CheckIfStructIsNull))]
        bool CheckIfRefStructIsNull(ref TestStruct? testStruct);

        int GetValueInNullableRefStruct(ref TestStruct? testStruct);
        void SetValueInNullableRefStruct(ref TestStruct? testStruct);

        string GetAFromStructAsString(ref TestStruct? testStruct);
        int GetAFromStructMultipliedByParameter(ref TestStruct? testStruct, int multiplier);
    }
}
