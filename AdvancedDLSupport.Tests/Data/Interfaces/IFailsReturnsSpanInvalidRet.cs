using System;
using System.Runtime.InteropServices;

#pragma warning disable SA1600, CS1591

namespace AdvancedDLSupport.Tests.Data
{
    public interface IFailsReturnsSpanInvalidRet
    {
        [NativeSymbol(Entrypoint = nameof(IReturnsSpanTests.GetInt32ArrayZeroToNine))]
        [return: NativeCollectionLength(10)]
        Span<object> InvalidSpan();
    }
}