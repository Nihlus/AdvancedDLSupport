// ReSharper disable UnusedMember.Global
namespace AdvanceDLSupport.Tests.Interfaces
{
    public interface ILazyLoadingLibrary
    {
        int Multiply(int a, int b);

        int MissingMethod(int a, int b);
        int MissingProperty { get; set; }
    }
}
