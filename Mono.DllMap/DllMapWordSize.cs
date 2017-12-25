using System;
using JetBrains.Annotations;

#pragma warning disable CS1591, SA1600, SA1602, SA1025

namespace Mono.DllMap
{
    [Flags, PublicAPI]
    public enum DllMapWordSize
    {
        Word32 = 1 << 0,
        Word64 = 1 << 1
    }
}
