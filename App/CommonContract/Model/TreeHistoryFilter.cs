using System;

namespace ProjectBranchSelector.Models
{
    public class TreeHistoryFilter : HistoryFilterBase<TreeHistoryModel>
    {
        public TreeHistoryFilter(Guid? id, DateTimeOffset? from, DateTimeOffset? to, 
            string name, int? page, int? size, string sort, bool withExtension = false)
            : base(id, from, to, name, page, size, sort, withExtension) { }
    }    
}
