using ProjectBranchSelector.Db.Model.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ProjectBranchSelector.Db.Interface
{
    public interface IRepository<TModel> where TModel : Entity
    {
        Task<TModel> AddAsync(TModel entity, CancellationToken cancellationToken, bool withSave = false);
        Task<TModel> GetAsync(Guid id, CancellationToken cancellationToken);
        IQueryable<TModel> GetAll();
        Task<TModel> RemoveAsync(Guid id, CancellationToken cancellationToken, bool withSave = false);
        Task<TModel> UpdateAsync(TModel entity, CancellationToken cancellationToken, bool withSave = false);
        Task<(int, IEnumerable<TModel>)> GetAsync(Filter<TModel> filter, CancellationToken cancellationToken);
        Task SaveChangesAsync();
    }

    public interface IRepositoryHistory<TModel> where TModel : Entity
    {        
        Task<TModel> GetAsync(Guid id, CancellationToken cancellationToken);
        IQueryable<TModel> GetAll();        
        Task<(int, IEnumerable<TModel>)> GetAsync(Filter<TModel> filter, CancellationToken cancellationToken);
        Task SaveChangesAsync();
    }
}