namespace AdvancedDLSupport.Tests.TestBases
{
    public abstract class LibraryTestBase<T> where T : class
    {
        protected readonly ImplementationOptions _config;
        protected readonly T _library;

        public LibraryTestBase(string libraryLocation)
        {
            _config = GetImplementationOptions();
            _library = GetImplementationBuilder().ResolveAndActivateInterface<T>(libraryLocation);
        }

        protected virtual ImplementationOptions GetImplementationOptions()
        {
            return 0;
        }

        protected virtual AnonymousImplementationBuilder GetImplementationBuilder()
        {
            return new AnonymousImplementationBuilder(_config);
        }
    }
}