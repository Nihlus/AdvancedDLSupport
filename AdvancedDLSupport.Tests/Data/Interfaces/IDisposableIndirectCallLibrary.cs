using System;

namespace AdvancedDLSupport.Tests.Data
{
    public interface IDisposableIndirectCallLibrary : IIndirectCallLibrary, IDisposable
    {
    }
}
