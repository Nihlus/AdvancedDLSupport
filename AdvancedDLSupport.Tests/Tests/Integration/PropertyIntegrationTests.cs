using System;
using System.Runtime.InteropServices;
using AdvancedDLSupport.Tests.Data;
using AdvancedDLSupport.Tests.TestBases;
using Xunit;

// ReSharper disable ArgumentsStyleLiteral

namespace AdvancedDLSupport.Tests.Integration
{
	public class PropertyIntegrationTests : LibraryTestBase<IPropertyLibrary>
	{
		private const string LibraryName = "PropertyTests";

		public PropertyIntegrationTests() : base(LibraryName)
		{
		}

		[Fact]
		public void CanGetGlobalVariableAsProperty()
		{
			Library.ResetData();

			Assert.Equal(5, Library.GlobalVariable);
		}

		[Fact]
		public void CanSetGlobalVariableAsProperty()
		{
			Library.ResetData();

			Library.GlobalVariable = 1;
			Assert.Equal(1, Library.GlobalVariable);
		}

		[Fact]
		public void CanGetGlobalVariableAsGetOnlyProperty()
		{
			Library.ResetData();

			Assert.Equal(5, Library.GlobalVariableGetOnly);
		}

		[Fact]
		public void CanSetGlobalVariableAsSetOnlyProperty()
		{
			Library.ResetData();

			Library.GlobalVariableSetOnly = 1;
			Assert.Equal(1, Library.GlobalVariable);
		}

		[Fact]
		public unsafe void CanGetGlobalPointerVariableAsProperty()
		{
			Library.ResetData();

			Assert.Equal(20, *Library.GlobalPointerVariable);
		}

		[Fact]
		public unsafe void CanSetGlobalPointerVariableAsProperty()
		{
			Library.ResetData();

			*Library.GlobalPointerVariable = 25;
			Assert.Equal(25, *Library.GlobalPointerVariable);
		}

		[Fact]
		public unsafe void CanGetGlobalPointerVariableAsGetOnlyProperty()
		{
			Library.ResetData();

			Assert.Equal(20, *Library.GlobalPointerVariableGetOnly);
		}

		[Fact]
		public unsafe void CanSetGlobalPointerVariableAsSetOnlyProperty()
		{
			Library.ResetData();

			Marshal.StructureToPtr(25, new IntPtr(Library.GlobalPointerVariableGetOnly), false);
			Assert.Equal(25, *Library.GlobalPointerVariable);
		}
	}
}