namespace AdvancedDLSupport.Tests.Data
{
    public interface INullableLibrary
    {
        TestStruct? GetAllocatedTestStruct();
        TestStruct? GetNullTestStruct();
        bool CheckIfStructIsNull(TestStruct? testStruct);

        [NativeSymbol(nameof(CheckIfStructIsNull))]
        bool CheckIfRefStructIsNull(ref TestStruct? testStruct);

        int GetValueInNullableRefStruct(ref TestStruct? testStruct);
        void SetValueInNullableRefStruct(ref TestStruct? testStruct);
    }
}
