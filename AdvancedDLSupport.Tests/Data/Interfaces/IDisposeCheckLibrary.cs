using System;

// ReSharper disable UnusedMember.Global

namespace AdvancedDLSupport.Tests.Data
{
    public interface IDisposeCheckLibrary : IDisposable
    {
        int Multiply(int a, int b);
    }
}
