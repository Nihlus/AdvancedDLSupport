using System.Runtime.InteropServices;

namespace AdvancedDLSupport.Tests.Data
{
    public interface IAttributePassthroughLibrary
    {
        [return: MarshalAs(UnmanagedType.I1)]
        bool CheckIfGreaterThanZero(int value);

        [return: MarshalAs(UnmanagedType.I1)]
        bool CheckIfStringIsNull(string value);

        [return: MarshalAs(UnmanagedType.LPWStr)]
        string EchoWString([MarshalAs(UnmanagedType.LPWStr)] string value);
    }
}
