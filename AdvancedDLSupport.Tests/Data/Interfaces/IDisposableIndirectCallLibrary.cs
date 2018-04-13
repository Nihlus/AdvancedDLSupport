using System;
using System.Runtime.InteropServices;

namespace AdvancedDLSupport.Tests.Data
{
    public interface IDisposableIndirectCallLibrary : IIndirectCallLibrary, IDisposable
    {
    }
}
