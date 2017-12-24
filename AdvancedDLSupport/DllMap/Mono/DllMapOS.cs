using System;
using JetBrains.Annotations;

#pragma warning disable CS1591, SA1600, SA1602, SA1025

namespace AdvancedDLSupport.DllMap.Mono
{
    [Flags, PublicAPI]
    internal enum DllMapOS
    {
        Linux     = 1 << 0,
        OSX       = 1 << 1,
        Solaris   = 1 << 2,
        FreeBSD   = 1 << 3,
        OpenBSD   = 1 << 4,
        NetBSD    = 1 << 5,
        Windows   = 1 << 6,
        AIX       = 1 << 7,
        HPUX      = 1 << 8
    }
}
