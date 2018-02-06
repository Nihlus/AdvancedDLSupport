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
			_library.ResetData();

			Assert.Equal(5, _library.GlobalVariable);
		}

		[Fact]
		public void CanSetGlobalVariableAsProperty()
		{
			_library.ResetData();

			_library.GlobalVariable = 1;
			Assert.Equal(1, _library.GlobalVariable);
		}

		[Fact]
		public void CanGetGlobalVariableAsGetOnlyProperty()
		{
			_library.ResetData();

			Assert.Equal(5, _library.GlobalVariableGetOnly);
		}

		[Fact]
		public void CanSetGlobalVariableAsSetOnlyProperty()
		{
			_library.ResetData();

			_library.GlobalVariableSetOnly = 1;
			Assert.Equal(1, _library.GlobalVariable);
		}

		[Fact]
		public unsafe void CanGetGlobalPointerVariableAsProperty()
		{
			_library.ResetData();

			Assert.Equal(20, *_library.GlobalPointerVariable);
		}

		[Fact]
		public unsafe void CanSetGlobalPointerVariableAsProperty()
		{
			_library.ResetData();

			*_library.GlobalPointerVariable = 25;
			Assert.Equal(25, *_library.GlobalPointerVariable);
		}

		[Fact]
		public unsafe void CanGetGlobalPointerVariableAsGetOnlyProperty()
		{
			_library.ResetData();

			Assert.Equal(20, *_library.GlobalPointerVariableGetOnly);
		}

		[Fact]
		public unsafe void CanSetGlobalPointerVariableAsSetOnlyProperty()
		{
			_library.ResetData();

			Marshal.StructureToPtr(25, new IntPtr(_library.GlobalPointerVariableGetOnly), false);
			Assert.Equal(25, *_library.GlobalPointerVariable);
		}
	}
}