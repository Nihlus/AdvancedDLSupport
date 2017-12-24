using System;
using JetBrains.Annotations;

// ReSharper disable InconsistentNaming
#pragma warning disable CS1591, SA1600, SA1602, SA1025, SA1300

namespace AdvancedDLSupport.DllMap.Mono
{
    [Flags, PublicAPI]
    internal enum DllMapArchitecture
    {
        x86    = 1 << 0,
        x86_64 = 1 << 1,
        SPARC  = 1 << 2,
        PPC    = 1 << 3,
        S390   = 1 << 4,
        S390X  = 1 << 5,
        ARM    = 1 << 6,
        ARMV8  = 1 << 7,
        MIPS   = 1 << 8,
        Alpha  = 1 << 9,
        HPPA   = 1 << 10,
        IA64   = 1 << 11
    }
}
