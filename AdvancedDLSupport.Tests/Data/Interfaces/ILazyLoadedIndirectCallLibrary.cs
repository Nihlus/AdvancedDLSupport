namespace AdvancedDLSupport.Tests.Data
{
    public interface ILazyLoadedIndirectCallLibrary : IIndirectCallLibrary
    {
        int MissingMethod();
    }
}
