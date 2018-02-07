using System;
using System.Runtime.InteropServices;

namespace AdvancedDLSupport.Tests.Data
{
    public interface ITypeLoweringLibrary
    {
        string GetString();
        string GetNullString();

        [return: MarshalAs(UnmanagedType.I1)]
        bool CheckIfStringIsNull(string value);

        UIntPtr StringLength(string value);
    }
}
