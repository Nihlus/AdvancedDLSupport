using System;
using System.Runtime.InteropServices;
using AdvancedDLSupport.Tests.Data;
using Xunit;

// ReSharper disable ArgumentsStyleLiteral

namespace AdvancedDLSupport.Tests.Integration
{
	public class PropertyIntegrationTests
	{
		private const string LibraryName = "PropertyTests";

		[Fact]
		public void CanGetGlobalVariableAsProperty()
		{
			var library = new AnonymousImplementationBuilder().ResolveAndActivateInterface<IPropertyLibrary>(LibraryName);
			library.ResetData();

			Assert.Equal(5, library.GlobalVariable);
		}

		[Fact]
		public void CanSetGlobalVariableAsProperty()
		{
			var library = new AnonymousImplementationBuilder().ResolveAndActivateInterface<IPropertyLibrary>(LibraryName);
			library.ResetData();

			library.GlobalVariable = 1;
			Assert.Equal(1, library.GlobalVariable);
		}

		[Fact]
		public void CanGetGlobalVariableAsGetOnlyProperty()
		{
			var library = new AnonymousImplementationBuilder().ResolveAndActivateInterface<IPropertyLibrary>(LibraryName);
			library.ResetData();

			Assert.Equal(5, library.GlobalVariableGetOnly);
		}

		[Fact]
		public void CanSetGlobalVariableAsSetOnlyProperty()
		{
			var library = new AnonymousImplementationBuilder().ResolveAndActivateInterface<IPropertyLibrary>(LibraryName);
			library.ResetData();

			library.GlobalVariableSetOnly = 1;
			Assert.Equal(1, library.GlobalVariable);
		}

		[Fact]
		public unsafe void CanGetGlobalPointerVariableAsProperty()
		{
			var library = new AnonymousImplementationBuilder().ResolveAndActivateInterface<IPropertyLibrary>(LibraryName);
			library.ResetData();

			Assert.Equal(20, *library.GlobalPointerVariable);
		}

		[Fact]
		public unsafe void CanSetGlobalPointerVariableAsProperty()
		{
			var library = new AnonymousImplementationBuilder().ResolveAndActivateInterface<IPropertyLibrary>(LibraryName);
			library.ResetData();

			*library.GlobalPointerVariable = 25;
			Assert.Equal(25, *library.GlobalPointerVariable);
		}

		[Fact]
		public unsafe void CanGetGlobalPointerVariableAsGetOnlyProperty()
		{
			var library = new AnonymousImplementationBuilder().ResolveAndActivateInterface<IPropertyLibrary>(LibraryName);
			library.ResetData();

			Assert.Equal(20, *library.GlobalPointerVariableGetOnly);
		}

		[Fact]
		public unsafe void CanSetGlobalPointerVariableAsSetOnlyProperty()
		{
			var library = new AnonymousImplementationBuilder().ResolveAndActivateInterface<IPropertyLibrary>(LibraryName);
			library.ResetData();

			Marshal.StructureToPtr(25, new IntPtr(library.GlobalPointerVariableGetOnly), false);
			Assert.Equal(25, *library.GlobalPointerVariable);
		}
	}
}