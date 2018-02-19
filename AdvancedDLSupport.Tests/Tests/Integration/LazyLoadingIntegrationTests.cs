using AdvancedDLSupport.Tests.Data;
using AdvancedDLSupport.Tests.TestBases;
using Xunit;

// ReSharper disable ArgumentsStyleLiteral
namespace AdvancedDLSupport.Tests.Integration
{
    public class LazyLoadingIntegrationTests : LibraryTestBase<ILazyLoadingLibrary>
    {
	    private const string LibraryName = "LazyLoadingTests";


	    public LazyLoadingIntegrationTests() : base(LibraryName)
	    {
	    }

	    protected override ImplementationOptions GetImplementationOptions()
	    {
		    return ImplementationOptions.UseLazyBinding;
	    }

	    [Fact]
		public void LoadingAnInterfaceWithAMissingFunctionThrows()
		{
			Assert.Throws<SymbolLoadingException>
			(
				() =>
					new NativeLibraryBuilder().ActivateInterface<ILazyLoadingLibrary>(LibraryName)
			);
		}

		[Fact]
		public void CallingMissingMethodInLazyLoadedInterfaceThrows()
		{
			Assert.Throws<SymbolLoadingException>
			(
				() =>
					Library.MissingMethod(0, 0)
			);
		}

		[Fact]
		public void LoadingAnInterfaceWithAMissingPropertyThrows()
		{
			Assert.Throws<SymbolLoadingException>
			(
				() =>
					new NativeLibraryBuilder().ActivateInterface<ILazyLoadingLibrary>(LibraryName)
			);
		}

		[Fact]
		public void SettingMissingPropertyInLazyLoadedInterfaceThrows()
		{
			Assert.Throws<SymbolLoadingException>
			(
				() =>
					Library.MissingProperty = 0
			);
		}

		[Fact]
		public void GettingMissingPropertyInLazyLoadedInterfaceThrows()
		{
			Assert.Throws<SymbolLoadingException>
			(
				() =>
					Library.MissingProperty
			);
		}
    }
}