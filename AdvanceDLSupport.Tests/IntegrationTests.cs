using System;
using AdvancedDLSupport;
using AdvanceDLSupport.Tests.Interfaces;
using AdvanceDLSupport.Tests.Structures;
using FsCheck.Xunit;
using Xunit;
// ReSharper disable ArgumentsStyleLiteral

namespace AdvanceDLSupport.Tests
{
	public class IntegrationTests
	{
		private const string LibraryName = "Test";

		[Fact]
		public void CanLoadLibrary()
		{
			var library = new AnonymousImplementationBuilder().ResolveAndActivateInterface<ITestLibrary>(LibraryName);
			Assert.NotNull(library);
		}

		[Fact]
		public void LoadingSameInterfaceAndSameFileTwiceProducesIdenticalReferences()
		{
			var firstLoad = new AnonymousImplementationBuilder().ResolveAndActivateInterface<ITestLibrary>(LibraryName);
			var secondLoad = new AnonymousImplementationBuilder().ResolveAndActivateInterface<ITestLibrary>(LibraryName);

			Assert.Same(firstLoad, secondLoad);
		}

		[Fact]
		public void LoadingSameInterfaceAndSameFileButWithDifferentOptionsProducesDifferentReferences()
		{
			var options = new ImplementationConfiguration(true);
			var firstLoad = new AnonymousImplementationBuilder(options).ResolveAndActivateInterface<ITestLibrary>(LibraryName);

			var secondLoad = new AnonymousImplementationBuilder().ResolveAndActivateInterface<ITestLibrary>(LibraryName);

			Assert.NotSame(firstLoad, secondLoad);
		}

		[Property]
		public void CanCallFunctionWithStructParameter(int value, int multiplier)
		{
			var library = new AnonymousImplementationBuilder().ResolveAndActivateInterface<ITestLibrary>(LibraryName);

			var strct =  new TestStruct { A = value };

			var expected = value * multiplier;
			var actual = library.DoStructMath(ref strct, multiplier);

			Assert.Equal(expected, actual);
		}

		[Property]
		public void CanCallFunctionWithSimpleParameter(int value, int multiplier)
		{
			var library = new AnonymousImplementationBuilder().ResolveAndActivateInterface<ITestLibrary>(LibraryName);

			var expected = value * multiplier;
			var actual = library.Multiply(value, multiplier);

			Assert.Equal(expected, actual);
		}

		[Property]
		public void CanCallFunctionWithDifferentEntryPoint(int value, int multiplier)
		{
			var library = new AnonymousImplementationBuilder().ResolveAndActivateInterface<ITestLibrary>(LibraryName);

			var strct =  new TestStruct { A = value };

			var expected = value * multiplier;
			var actual = library.Multiply(ref strct, multiplier);

			Assert.Equal(expected, actual);
		}

		[Property]
		public void CanCallFunctionWithDifferentCallingConvention(int value, int other)
		{
			var library = new AnonymousImplementationBuilder().ResolveAndActivateInterface<ITestLibrary>(LibraryName);

			var expected = value - other;
			var actual = library.CDeclSubtract(value, other);

			Assert.Equal(expected, actual);
		}

		[Property]
		public void CanCallDuplicateFunction(int value, int other)
		{
			var library = new AnonymousImplementationBuilder().ResolveAndActivateInterface<ITestLibrary>(LibraryName);

			var expected = value - other;
			var actual = library.Subtract(value, other);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void CanGetGlobalVariableAsProperty()
		{
			var library = new AnonymousImplementationBuilder().ResolveAndActivateInterface<ITestLibrary>(LibraryName);

			Assert.Equal(5, library.GlobalVariableA);
		}

		[Fact]
		public void CanSetGlobalVariableAsProperty()
		{
			var library = new AnonymousImplementationBuilder().ResolveAndActivateInterface<ITestLibrary>(LibraryName);

			library.GlobalVariableA = 1;
			Assert.Equal(1, library.GlobalVariableA);
		}

		[Fact]
		public unsafe void CanGetGlobalPointerVariableAsProperty()
		{
			var library = new AnonymousImplementationBuilder().ResolveAndActivateInterface<ITestLibrary>(LibraryName);

			library.InitializeGlobalPointerVariable();
			Assert.Equal(20, *library.GlobalPointerVariable);
		}

		[Fact]
		public unsafe void CanSetGlobalPointerVariableAsProperty()
		{
			var library = new AnonymousImplementationBuilder().ResolveAndActivateInterface<ITestLibrary>(LibraryName);

			library.InitializeGlobalPointerVariable();
			*library.GlobalPointerVariable = 25;
			Assert.Equal(25, *library.GlobalPointerVariable);
		}

		[Fact]
		public void LoadingAnInterfaceWithAMissingFunctionThrows()
		{
			Assert.Throws<SymbolLoadingException>
			(
				() =>
					new AnonymousImplementationBuilder().ResolveAndActivateInterface<ITestLibraryMissingMethod>(LibraryName)
			);
		}

		[Fact]
		public void LazyLoadingAnInterfaceWithAMissingMethodDoesNotThrow()
		{
			var config = new ImplementationConfiguration(useLazyBinding:true);
			var library = new AnonymousImplementationBuilder(config).ResolveAndActivateInterface<ITestLibraryMissingMethod>(LibraryName);
		}

		[Fact]
		public void CallingMissingMethodInLazyLoadedInterfaceThrows()
		{
			var config = new ImplementationConfiguration(useLazyBinding:true);
			var library = new AnonymousImplementationBuilder(config).ResolveAndActivateInterface<ITestLibraryMissingMethod>(LibraryName);

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
					new AnonymousImplementationBuilder().ResolveAndActivateInterface<ITestLibraryMissingProperty>(LibraryName)
			);
		}

		[Fact]
		public void LazyLoadingAnInterfaceWithAMissingPropertyDoesNotThrow()
		{
			var config = new ImplementationConfiguration(useLazyBinding:true);
			var library = new AnonymousImplementationBuilder(config).ResolveAndActivateInterface<ITestLibraryMissingProperty>(LibraryName);
		}

		[Fact]
		public void SettingMissingPropertyInLazyLoadedInterfaceThrows()
		{
			var config = new ImplementationConfiguration(useLazyBinding:true);
			var library = new AnonymousImplementationBuilder(config).ResolveAndActivateInterface<ITestLibraryMissingProperty>(LibraryName);

			Assert.Throws<SymbolLoadingException>
			(
				() =>
					library.MissingProperty = 0
			);
		}

		[Fact]
		public void GettingMissingPropertyInLazyLoadedInterfaceThrows()
		{
			var config = new ImplementationConfiguration(useLazyBinding:true);
			var library = new AnonymousImplementationBuilder(config).ResolveAndActivateInterface<ITestLibraryMissingProperty>(LibraryName);

			Assert.Throws<SymbolLoadingException>
			(
				() =>
					library.MissingProperty
			);
		}

		[Fact]
		public void DisposedLibraryWithoutGeneratedChecksDoesNotThrow()
		{
			var library = new AnonymousImplementationBuilder().ResolveAndActivateInterface<ITestLibrary>(LibraryName);
			library.Dispose();
			library.Multiply(5, 5);
		}

		[Fact]
		public void UndisposedLibraryDoesNotThrow()
		{
			var config = new ImplementationConfiguration(generateDisposalChecks:true);
			var library = new AnonymousImplementationBuilder(config).ResolveAndActivateInterface<ITestLibrary>(LibraryName);

			library.Multiply(5, 5);
		}

		[Fact]
		public void DisposedLibraryThrows()
		{
			var config = new ImplementationConfiguration(generateDisposalChecks:true);
			var library = new AnonymousImplementationBuilder(config).ResolveAndActivateInterface<ITestLibrary>(LibraryName);
			library.Dispose();

			Assert.Throws<ObjectDisposedException>(() => library.Multiply(5, 5));
		}
	}
}