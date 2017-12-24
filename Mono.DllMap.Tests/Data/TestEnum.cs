using System;

namespace Mono.DllMap.Tests.Data
{
    [Flags]
    public enum TestEnum
    {
        Foo = 1 << 0,
        Bar = 1 << 1,
        Baz = 1 << 2
    }
}
