using ProjectBranchSelector.DbClient.Model.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ProjectBranchSelector.DbClient.Interface
{
    public interface IRepository<TModel> where TModel : Entity
    {
        TModel Add(TModel entity, bool withSave = false);
        TModel Get(Guid id, bool withDeleted = false);
        IQueryable<TModel> GetAll(bool withDeleted = false);
        TModel Remove(Guid id, bool withSave = false);
        TModel Update(TModel entity, bool withSave = false);
        IEnumerable<TModel> Get(Filter<TModel> filterr, bool withDeleted = false);
    }
}