namespace Mono.DllMap.Tests.Data
{
    public enum TestEnumWithoutFlagAttribute
    {
        Foo = 1 << 0,
        Bar = 1 << 1,
        Baz = 1 << 2
    }
}
