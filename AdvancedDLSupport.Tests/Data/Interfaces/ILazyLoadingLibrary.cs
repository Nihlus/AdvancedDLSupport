// ReSharper disable UnusedMember.Global
namespace AdvancedDLSupport.Tests.Data
{
    public interface ILazyLoadingLibrary
    {
        int Multiply(int a, int b);

        int MissingMethod(int a, int b);
        int MissingProperty { get; set; }
    }
}
