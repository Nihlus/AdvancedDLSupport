using System;
using System.Runtime.InteropServices;

#pragma warning disable SA1600, CS1591

namespace AdvancedDLSupport.Tests.Data
{
    public interface IFailsReturnsSpanNoAttr
    {
        [NativeSymbol(CallingConvention = CallingConvention.StdCall, Entrypoint = nameof(IReturnsSpanTests.ReturnsInt32ArrayZeroToNine))]
        Span<int> NoAttr();
    }
}