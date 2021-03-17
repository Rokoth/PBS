using System;

namespace ProjectBranchSelector.Models
{
    public class TreeItemsHistoryFilter : Filter<TreeItemHistoryModel>, INamedFilter, IHistoryFilter
    {
        public TreeItemsHistoryFilter(Guid? id, DateTimeOffset? from, DateTimeOffset? to, string name, int? page, int? size, string sort, bool withExtension = false)
            : base(page, size, sort, withExtension)
        {
            Id = id;
            From = from;
            To = to;
            Name = name;
        }

        public Guid TreeId { get; set; }
        public DateTimeOffset? From { get; set; }
        public DateTimeOffset? To { get; set; }
        public string Name { get; set; }
        public Guid? Id { get; set; }
    }


}
