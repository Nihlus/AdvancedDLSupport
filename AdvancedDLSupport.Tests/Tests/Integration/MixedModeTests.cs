using System;
using AdvancedDLSupport.Tests.Data;
using AdvancedDLSupport.Tests.Data.Classes;
using Xunit;

namespace AdvancedDLSupport.Tests.Integration
{
	public class MixedModeTests
	{
		private const string LibraryName = "MixedModeTests";

		private readonly MixedModeClass MixedModeClass;
		private readonly AnonymousImplementationBuilder Builder;

		public MixedModeTests()
		{
			this.Builder = new AnonymousImplementationBuilder();

			this.MixedModeClass = this.Builder.ResolvedAndActivateClass<MixedModeClass, IMixedModeLibrary>(LibraryName);
		}

		[Fact]
		public void ThrowsIfClassDoesNotInheritFromAnonymousImplementationBase()
		{
			Assert.Throws<ArgumentException>
			(
				() =>
					Builder.ResolvedAndActivateClass<MixedModeClassThatDoesNotInheritFromAnonymousBase, IMixedModeLibrary>(LibraryName)
			);
		}

		[Fact]
		public void ThrowsIfClassIsNotAbstract()
		{
			Assert.Throws<ArgumentException>
			(
				() =>
					Builder.ResolvedAndActivateClass<MixedModeClassThatIsNotAbstract, IMixedModeLibrary>(LibraryName)
			);
		}

		[Fact]
		public void CanOverrideNativeFunctionWithManagedImplementationOnClass()
		{
			var result = this.MixedModeClass.Subtract(10, 5);

			Assert.Equal(5, result);
			Assert.True(this.MixedModeClass.RanManagedSubtract);
		}

		[Fact]
		public void CanCallPurelyManagedFunctionOnClass()
		{
			var result = this.MixedModeClass.ManagedAdd(5, 5);

			Assert.Equal(10, result);
		}

		[Fact]
		public void CanCallNativeFunctionOnClass()
		{
			var result = this.MixedModeClass.Multiply(5, 5);

			Assert.Equal(25, result);
		}

		[Fact]
		public void CanUseNativePropertyOnClass()
		{
			var originalValue = this.MixedModeClass.NativeProperty;
			Assert.Equal(0, originalValue);

			var newValue = 16;
			this.MixedModeClass.NativeProperty = newValue;
			Assert.Equal(newValue, this.MixedModeClass.NativeProperty);
		}

		[Fact]
		public void CanOverrideNativePropertyWithManagedImplementationOnClass()
		{
			Assert.Equal(32, this.MixedModeClass.OtherNativeProperty);

			this.MixedModeClass.OtherNativeProperty = 255;
			Assert.True(this.MixedModeClass.RanManagedSetter);
		}
	}
}