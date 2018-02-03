namespace AdvancedDLSupport.Tests.Data.Classes
{
	public abstract class MixedModeClassThatDoesNotInheritFromAnonymousBase : IMixedModeLibrary
	{
		public bool RanManagedSubtract { get; private set; }

		public int ManagedAdd(int a, int b)
		{
			return a + b;
		}

		public abstract int NativeProperty { get; set; }
		public abstract int OtherNativeProperty { get; set; }
		public abstract int Multiply(int value, int multiplier);

		public int Subtract(int value, int other)
		{
			RanManagedSubtract = true;
			return value - other;
		}
	}
}