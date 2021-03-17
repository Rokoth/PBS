namespace ProjectBranchSelector.Models
{
    public class FormulaFilter : Filter<FormulaModel>, INamedFilter
    {
        public FormulaFilter(string name, int? page, int? size, string sort, bool withExtension = false) : base(page, size, sort, withExtension)
        {
            Name = name;
        }

        public string Name { get; set; }
    }

}
