using AdvancedDLSupport.Tests.Data;
using Xunit;

// ReSharper disable ArgumentsStyleLiteral
namespace AdvancedDLSupport.Tests.Integration
{
    public class LazyLoadingIntegrationTests
    {
	    private const string LibraryName = "LazyLoadingTests";

        [Fact]
		public void LoadingAnInterfaceWithAMissingFunctionThrows()
		{
			Assert.Throws<SymbolLoadingException>
			(
				() =>
					new AnonymousImplementationBuilder().ResolveAndActivateInterface<ILazyLoadingLibrary>(LibraryName)
			);
		}

		[Fact]
		public void LazyLoadingAnInterfaceWithAMissingMethodDoesNotThrow()
		{
			var config = new ImplementationConfiguration
			{
				UseLazyBinding = true
			};
			new AnonymousImplementationBuilder(config).ResolveAndActivateInterface<ILazyLoadingLibrary>(LibraryName);
		}

		[Fact]
		public void CallingMissingMethodInLazyLoadedInterfaceThrows()
		{
			var config = new ImplementationConfiguration
			{
				UseLazyBinding = true
			};
			var library = new AnonymousImplementationBuilder(config).ResolveAndActivateInterface<ILazyLoadingLibrary>(LibraryName);

			Assert.Throws<SymbolLoadingException>
			(
				() =>
					library.MissingMethod(0, 0)
			);
		}

		[Fact]
		public void LoadingAnInterfaceWithAMissingPropertyThrows()
		{
			Assert.Throws<SymbolLoadingException>
			(
				() =>
					new AnonymousImplementationBuilder().ResolveAndActivateInterface<ILazyLoadingLibrary>(LibraryName)
			);
		}

		[Fact]
		public void LazyLoadingAnInterfaceWithAMissingPropertyDoesNotThrow()
		{
			var config = new ImplementationConfiguration
			{
				UseLazyBinding = true
			};
			new AnonymousImplementationBuilder(config).ResolveAndActivateInterface<ILazyLoadingLibrary>(LibraryName);
		}

		[Fact]
		public void SettingMissingPropertyInLazyLoadedInterfaceThrows()
		{
			var config = new ImplementationConfiguration
			{
				UseLazyBinding = true
			};
			var library = new AnonymousImplementationBuilder(config).ResolveAndActivateInterface<ILazyLoadingLibrary>(LibraryName);

			Assert.Throws<SymbolLoadingException>
			(
				() =>
					library.MissingProperty = 0
			);
		}

		[Fact]
		public void GettingMissingPropertyInLazyLoadedInterfaceThrows()
		{
			var config = new ImplementationConfiguration
			{
				UseLazyBinding = true
			};
			var library = new AnonymousImplementationBuilder(config).ResolveAndActivateInterface<ILazyLoadingLibrary>(LibraryName);

			Assert.Throws<SymbolLoadingException>
			(
				() =>
					library.MissingProperty
			);
		}
    }
}