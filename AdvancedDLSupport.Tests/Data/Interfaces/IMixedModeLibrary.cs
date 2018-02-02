namespace AdvancedDLSupport.Tests.Data
{
	public interface IMixedModeLibrary
	{
		int NativeProperty { get; set; }
		int OtherNativeProperty { get; set; }

		int Multiply(int a, int b);
		int Subtract(int a, int b);
	}
}