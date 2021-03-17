namespace ProjectBranchSelector.Models
{
    public interface IPagedFilter
    {
        int? Page { get; set; }
        int? Size { get; set; }
        string Sort { get; set; }
        bool WithExtension { get; set; }
    }
}