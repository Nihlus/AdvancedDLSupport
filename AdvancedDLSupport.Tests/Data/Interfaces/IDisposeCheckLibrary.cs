using System;

namespace AdvancedDLSupport.Tests.Data
{
    public interface IDisposeCheckLibrary : IDisposable
    {
        int Multiply(int a, int b);
    }
}
