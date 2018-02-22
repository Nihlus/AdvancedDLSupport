using JetBrains.Annotations;

namespace AdvancedDLSupport.Tests.TestBases
{
    public class TypeTransformerRepositoryTestBase
    {
        [NotNull]
        protected TypeTransformerRepository Repository { get; }

        protected TypeTransformerRepositoryTestBase()
        {
            Repository = new TypeTransformerRepository();
        }
    }
}
