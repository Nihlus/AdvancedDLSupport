using System;
using AdvancedDLSupport.Tests.Data;
using AdvancedDLSupport.Tests.Data.Classes;
using Xunit;

namespace AdvancedDLSupport.Tests.Integration
{
	public class MixedModeTests
	{
		private const string LibraryName = "MixedModeTests";

		private readonly MixedModeClass _mixedModeClass;
		private readonly NativeLibraryBuilder _builder;

		public MixedModeTests()
		{
			_builder = new NativeLibraryBuilder();

			_mixedModeClass = _builder.ActivateClass<MixedModeClass, IMixedModeLibrary>(LibraryName);
		}

		[Fact]
		public void ThrowsIfClassIsNotAbstract()
		{
			Assert.Throws<ArgumentException>
			(
				() =>
					_builder.ActivateClass<MixedModeClassThatIsNotAbstract, IMixedModeLibrary>(LibraryName)
			);
		}

		[Fact]
		public void CanOverrideNativeFunctionWithManagedImplementationOnClass()
		{
			var result = _mixedModeClass.Subtract(10, 5);

			Assert.Equal(5, result);
			Assert.True(_mixedModeClass.RanManagedSubtract);
		}

		[Fact]
		public void CanCallPurelyManagedFunctionOnClass()
		{
			var result = _mixedModeClass.ManagedAdd(5, 5);

			Assert.Equal(10, result);
		}

		[Fact]
		public void CanCallNativeFunctionOnClass()
		{
			var result = _mixedModeClass.Multiply(5, 5);

			Assert.Equal(25, result);
		}

		[Fact]
		public void CanUseNativePropertyOnClass()
		{
			var originalValue = _mixedModeClass.NativeProperty;
			Assert.Equal(0, originalValue);

			var newValue = 16;
			_mixedModeClass.NativeProperty = newValue;
			Assert.Equal(newValue, _mixedModeClass.NativeProperty);
		}

		[Fact]
		public void CanOverrideNativePropertyWithManagedImplementationOnClass()
		{
			Assert.Equal(32, _mixedModeClass.OtherNativeProperty);

			_mixedModeClass.OtherNativeProperty = 255;
			Assert.True(_mixedModeClass.RanManagedSetter);
		}
	}
}