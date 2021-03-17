using System;
using System.Linq.Expressions;

namespace ProjectBranchSelector.DbClient.Model.Common
{
    public class Filter<TModel> where TModel : Entity
    {
        public int Page { get; set; }
        public int Size { get; set; }
        public string Sort { get; set; }
        public Expression<Func<TModel, bool>> Selector { get; set; }
    }
}
