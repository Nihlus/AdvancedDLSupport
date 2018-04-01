using System;

// ReSharper disable ValueParameterNotUsed
namespace AdvancedDLSupport.Tests.Data.Classes
{
	public abstract class MixedModeClass : NativeLibraryBase, IMixedModeLibrary
	{
		public MixedModeClass(string path, Type interfaceType, ImplementationOptions options, TypeTransformerRepository transformerRepository)
			: base(path, interfaceType, options, transformerRepository)
		{
		}

		public bool RanManagedSubtract { get; private set; }

		public bool RanManagedSetter { get; private set; }

		public int ManagedAdd(int a, int b)
		{
			return a + b;
		}

		public int OtherNativeProperty
		{
			get => 32;

			set => RanManagedSetter = true;
		}

		public abstract int NativeProperty { get; set; }
		public abstract int Multiply(int value, int multiplier);

		public int Subtract(int value, int other)
		{
			RanManagedSubtract = true;
			return value - other;
		}
	}
}