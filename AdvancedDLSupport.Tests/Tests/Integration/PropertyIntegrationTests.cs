using System;
using System.Runtime.InteropServices;
using AdvancedDLSupport.Tests.Data;
using AdvancedDLSupport.Tests.TestBases;
using Xunit;

// ReSharper disable ArgumentsStyleLiteral

namespace AdvancedDLSupport.Tests.Integration
{
	public class PropertyIntegrationTests
	{
		private const string LibraryName = "PropertyTests";
		private const string TestCollectionName = "VolatilePropertyData";

		[Collection(TestCollectionName)]
		public class Getter : LibraryTestBase<IPropertyLibrary>
		{
			public Getter() : base(LibraryName)
			{
				Library.ResetData();
			}

			[Fact]
			public void CanGetGlobalVariableAsProperty()
			{
				Assert.Equal(5, Library.GlobalVariable);
			}

			[Fact]
			public void CanGetGlobalVariableAsGetOnlyProperty()
			{
				Assert.Equal(5, Library.GlobalVariableGetOnly);
			}

			[Fact]
			public unsafe void CanGetGlobalPointerVariableAsProperty()
			{
				Assert.Equal(20, *Library.GlobalPointerVariable);
			}

			[Fact]
			public unsafe void CanGetGlobalPointerVariableAsGetOnlyProperty()
			{
				Assert.Equal(20, *Library.GlobalPointerVariableGetOnly);
			}
		}

		[Collection(TestCollectionName)]
		public class Setter : LibraryTestBase<IPropertyLibrary>
		{
			public Setter() : base(LibraryName)
			{
				Library.ResetData();
			}

			[Fact]
			public void CanSetGlobalVariableAsProperty()
			{
				Library.GlobalVariable = 1;
				Assert.Equal(1, Library.GlobalVariable);
			}

			[Fact]
			public void CanSetGlobalVariableAsSetOnlyProperty()
			{
				Library.GlobalVariableSetOnly = 1;
				Assert.Equal(1, Library.GlobalVariable);
			}

			[Fact]
			public unsafe void CanSetGlobalPointerVariableAsProperty()
			{
				*Library.GlobalPointerVariable = 25;
				Assert.Equal(25, *Library.GlobalPointerVariable);
			}

			[Fact]
			public unsafe void CanSetGlobalPointerVariableAsSetOnlyProperty()
			{
				Marshal.StructureToPtr(25, new IntPtr(Library.GlobalPointerVariableGetOnly), false);
				Assert.Equal(25, *Library.GlobalPointerVariable);
			}
		}

		public class FailureCases
		{
			[Fact]
			public void ThrowsNotSupportedExceptionIfPropertyHasClassType()
			{
				var builder = NativeLibraryBuilder.Default;

				Assert.Throws<NotSupportedException>
				(
					() =>
						builder.ActivateInterface<IPropertyWithClassTypeLibrary>(LibraryName)
				);
			}
		}
	}
}