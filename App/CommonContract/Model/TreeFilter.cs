namespace ProjectBranchSelector.Models
{
    public class TreeFilter : Filter<TreeModel>, INamedFilter
    {
        public TreeFilter(string name, int? page, int? size, string sort, bool withExtension = false): base(page, size, sort, withExtension)
        {
            Name = name;
        }

        public string Name { get; set; }
    }
}
