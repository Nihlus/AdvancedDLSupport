using System;
using System.Runtime.InteropServices;

namespace AdvancedDLSupport.Tests.Data
{
    public interface IComplexTypeLibrary
    {
        string GetString();
        string GetNullString();

        [return: MarshalAs(UnmanagedType.I1)]
        bool CheckIfStringIsNull(string value);

        UIntPtr StringLength(string value);
        TestStruct? GetAllocatedTestStruct();
        TestStruct? GetNullTestStruct();

        [return: MarshalAs(UnmanagedType.I1)]
        bool CheckIfStructIsNull(TestStruct? testStruct);
    }
}
