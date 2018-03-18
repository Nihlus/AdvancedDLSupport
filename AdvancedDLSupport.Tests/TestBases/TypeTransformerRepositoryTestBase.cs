namespace AdvancedDLSupport.Tests.TestBases
{
    public class TypeTransformerRepositoryTestBase
    {
        protected TypeTransformerRepository Repository { get; }

        protected TypeTransformerRepositoryTestBase()
        {
            Repository = new TypeTransformerRepository();
        }
    }
}
