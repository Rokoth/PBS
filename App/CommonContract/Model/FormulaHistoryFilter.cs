using System;

namespace ProjectBranchSelector.Models
{    
    public class FormulaHistoryFilter : HistoryFilterBase<FormulaHistoryModel>
    {
        public FormulaHistoryFilter(Guid? id, DateTimeOffset? from, DateTimeOffset? to, string name, 
            int? page, int? size, string sort, bool withExtension = false)
            : base(id, from, to, name, page, size, sort, withExtension) { }
    }
}
