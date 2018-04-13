using System;
using AdvancedDLSupport.Tests.Data;
using AdvancedDLSupport.Tests.TestBases;
using Xunit;
using static AdvancedDLSupport.ImplementationOptions;

namespace AdvancedDLSupport.Tests.Integration
{
    public class IndirectCallsIntegrationTests
    {
        public class Simple : IndirectCallsTestBase<IIndirectCallLibrary>
        {
            protected override ImplementationOptions GetImplementationOptions()
            {
                return UseIndirectCalls;
            }
        }

        public class LazyBinding : IndirectCallsTestBase<ILazyLoadedIndirectCallLibrary>
        {
            protected override ImplementationOptions GetImplementationOptions()
            {
                return UseIndirectCalls | UseLazyBinding;
            }

            [Fact]
            public void ThrowsIfMissingMemberIsCalled()
            {
                Assert.Throws<SymbolLoadingException>(() => Library.MissingMethod());
            }
        }

        public class DisposalChecks : IndirectCallsTestBase<IDisposableIndirectCallLibrary>
        {
            protected override ImplementationOptions GetImplementationOptions()
            {
                return UseIndirectCalls | GenerateDisposalChecks;
            }

            [Fact]
            public void ThrowsIfMemberIsCalledOnDisposedObject()
            {
                Library.Dispose();

                Assert.Throws<ObjectDisposedException>(() => Library.Multiply(5, 5));
            }
        }
    }
}