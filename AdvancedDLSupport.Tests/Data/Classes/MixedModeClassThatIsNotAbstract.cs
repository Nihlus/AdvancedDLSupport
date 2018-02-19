using System;

namespace AdvancedDLSupport.Tests.Data.Classes
{
	public class MixedModeClassThatIsNotAbstract : NativeLibraryBase, IMixedModeLibrary
	{
		public MixedModeClassThatIsNotAbstract(string path, Type interfaceType, ImplementationOptions options, TypeTransformerRepository transformerRepository)
			: base(path, interfaceType, options, transformerRepository)
		{
		}

		public bool RanManagedSubtract { get; private set; }

		public int ManagedAdd(int a, int b)
		{
			return a + b;
		}

		public int NativeProperty { get; set; }
		public int OtherNativeProperty { get; set; }

		public int Multiply(int value, int multiplier)
		{
			return value * multiplier;
		}

		public int Subtract(int value, int other)
		{
			RanManagedSubtract = true;
			return value - other;
		}
	}
}