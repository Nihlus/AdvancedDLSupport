#pragma warning disable SA1600, CS1591 // Elements should be documented

namespace AdvancedDLSupport.Example
{
    public unsafe interface IExample
    {
        MyStruct* MyStructure { get; set; }

        void InitializeMyStructure();

        int DoMath(ref MyStruct struc);

        string GetString();

        string GetNullString();

        MyStruct? GetAllocatedTestStruct();
    }
}
