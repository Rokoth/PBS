namespace ProjectBranchSelector.Models
{
    public class Filter<T> : IPagedFilter where T : EntityModel
    {
        public Filter(int? page, int? size, string sort, bool withExtension = false)
        {
            Page = page;
            Size = size;
            Sort = sort;
            WithExtension = withExtension;
        }
        public int? Page { get; set; }
        public int? Size { get; set; }
        public string Sort { get; set; }
        public bool WithExtension { get; set; }
    }
}
