namespace Mono.DllMap.Tests.TestBases
{
    public class MapResolverTestBase
    {
        protected const string OriginalLibraryName = "cygwin1.dll";
        protected const string RemappedLibraryName = "libc.so.6";
        protected const string UnmappedLibraryName = "libunmapped.so";

        protected readonly DllMapResolver Resolver;

        protected MapResolverTestBase()
        {
            Resolver = new DllMapResolver();;
        }
    }
}
