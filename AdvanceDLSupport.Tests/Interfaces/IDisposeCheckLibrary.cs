using System;
// ReSharper disable UnusedMember.Global

namespace AdvanceDLSupport.Tests.Interfaces
{
    public interface IDisposeCheckLibrary : IDisposable
    {
        int Multiply(int a, int b);
    }
}
