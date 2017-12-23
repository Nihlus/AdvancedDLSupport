using System;
using AdvancedDLSupport;

namespace AdvanceDLSupport.Tests.Interfaces
{
    public unsafe interface IPropertyLibrary : IDisposable
    {
        void InitializeGlobalPointerVariable();
        void ResetData();

        [NativeSymbol(nameof(GlobalVariable))]
        int GlobalVariableSetOnly { set; }

        [NativeSymbol(nameof(GlobalVariable))]
        int GlobalVariableGetOnly { get; }

        int GlobalVariable { get; set; }

        [NativeSymbol(nameof(GlobalPointerVariable))]
        int* GlobalPointerVariableSetOnly { set; }

        [NativeSymbol(nameof(GlobalPointerVariable))]
        int* GlobalPointerVariableGetOnly { get; }

        int* GlobalPointerVariable { get; set; }
    }
}
