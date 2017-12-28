using System;
using System.Runtime.InteropServices;

// ReSharper disable UnusedMember.Global

namespace AdvancedDLSupport.Tests.Data
{
    public interface IComplexTypeLibrary
    {
        string GetString();
        string GetNullString();

        bool CheckIfStringIsNull(string value);
        UIntPtr StringLength(string value);
        TestStruct? GetAllocatedTestStruct();
        TestStruct? GetNullTestStruct();
        bool CheckIfStructIsNull(TestStruct? testStruct);
    }
}
