using System;
using System.Runtime.InteropServices;

namespace AdvancedDLSupport.Tests.Data
{
    public interface ITypeLoweringLibrary
    {
        string GetString();
        string GetNullString();

        [return: MarshalAs(UnmanagedType.BStr)]
        string EchoBStr([MarshalAs(UnmanagedType.BStr)] string value);

        [return: MarshalAs(UnmanagedType.LPTStr)]
        string GetLPTString();

        [return: MarshalAs(UnmanagedType.LPWStr)]
        string GetLPWString();

        [return: MarshalAs(UnmanagedType.I1)]
        bool CheckIfStringIsNull(string value);

        UIntPtr StringLength(string value);

        UIntPtr BStringLength([MarshalAs(UnmanagedType.BStr)] string value);

        UIntPtr LPTStringLength([MarshalAs(UnmanagedType.LPTStr)] string value);
        UIntPtr LPWStringLength([MarshalAs(UnmanagedType.LPWStr)] string value);
    }
}
