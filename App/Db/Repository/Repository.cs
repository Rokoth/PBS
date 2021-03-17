using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ProjectBranchSelector.Db.Context;
using ProjectBranchSelector.Db.Interface;
using ProjectBranchSelector.Db.Model.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading;
using System.Threading.Tasks;

namespace Db.Repository
{
    public class Repository<TModel> : IRepository<TModel> where TModel : Entity
    {
        private readonly DbPgContext context;
        private readonly ILogger logger;

        public Repository(IServiceProvider serviceProvider)
        {
            context = serviceProvider.GetRequiredService<DbPgContext>();
            logger = serviceProvider.GetRequiredService<ILogger<Repository<TModel>>>();
        }

        public IQueryable<TModel> GetAll()
        {
            return context.Set<TModel>().Where(s => !s.IsDeleted);
        }

        public async Task<TModel> GetAsync(Guid id, CancellationToken cancellationToken)
        {
            return await Execute((token) => {               
                return context.Set<TModel>().Where(s => s.Id == id && !s.IsDeleted).FirstOrDefaultAsync(cancellationToken);
            }, "GetAsync", cancellationToken);          
        }

        public async Task<TModel> AddAsync(TModel entity, CancellationToken cancellationToken, bool withSave = false)
        {
            return await Execute(async (token) => {
                entity.IsDeleted = false;
                entity.VersionDate = DateTimeOffset.Now;                
                var result = context.Set<TModel>().Add(entity).Entity;
                if (withSave)
                {
                    await context.SaveChangesAsync(cancellationToken);
                }
                return result;
            }, "AddAsync", cancellationToken);
        }

        public async Task<TModel> UpdateAsync(TModel entity, CancellationToken cancellationToken, bool withSave = false)
        {
            return await Execute(async (token) => {
                entity.VersionDate = DateTimeOffset.Now;                
                var result = context.Set<TModel>().Update(entity).Entity;
                if (withSave)
                {
                    await context.SaveChangesAsync(cancellationToken);
                }
                return result;
            }, "UpdateAsync", cancellationToken);           
        }

        public async Task<TModel> RemoveAsync(Guid id, CancellationToken cancellationToken, bool withSave = false)
        {
            return await Execute(async (token) => {
                var toRemove = await context.Set<TModel>()
                    .Where(s => s.Id == id && !s.IsDeleted).FirstOrDefaultAsync();
                toRemove.IsDeleted = true;
                var result = context.Set<TModel>().Update(toRemove).Entity;
                if (withSave)
                {
                    await context.SaveChangesAsync(cancellationToken);
                }
                return result;
            }, "RemoveAsync", cancellationToken);
        }

        public async Task<(int,IEnumerable<TModel>)> GetAsync(Filter<TModel> filter, CancellationToken cancellationToken)
        {
            return await Execute<(int, IEnumerable<TModel>)>(async (token)=> {                
                var all = context.Set<TModel>().Where(s => !s.IsDeleted).Where(filter.Selector);

                if (!string.IsNullOrEmpty(filter.Sort))
                {
                    all = all.OrderBy(filter.Sort);
                }
                var count = await all.CountAsync();
                if (filter.Page != null && filter.Size != null)
                {
                    all = all.Skip(filter.Page.Value * filter.Size.Value).Take(filter.Size.Value);
                }

                return (count, await all.ToListAsync(cancellationToken));
            }, "GetAsync", cancellationToken);            
        }

        private async Task<TResult> Execute<TResult>(Func<CancellationToken, Task<TResult>> action, string methodName, CancellationToken cancellationToken)
        {
            try
            {
                return await action(cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError($"Error in {methodName} method: {ex.Message} \r\nStackTrace: {ex.StackTrace}");
                throw new DatabaseException($"Error in {methodName} method: {ex.Message}");
            }
        }

        public async Task SaveChangesAsync()
        {
            await context.SaveChangesAsync();
        }
    }

    public class RepositoryHistory<TModel> : IRepositoryHistory<TModel> where TModel : EntityHistory
    {
        private readonly DbPgContext context;
        private readonly ILogger logger;

        public RepositoryHistory(IServiceProvider serviceProvider)
        {
            context = serviceProvider.GetRequiredService<DbPgContext>();
            logger = serviceProvider.GetRequiredService<ILogger<Repository<TModel>>>();
        }

        public IQueryable<TModel> GetAll()
        {
            return context.Set<TModel>();
        }

        public async Task<TModel> GetAsync(Guid id, CancellationToken cancellationToken)
        {
            return await Execute((token) => {
                return context.Set<TModel>().Where(s => s.Id == id).FirstOrDefaultAsync(cancellationToken);
            }, "GetAsync", cancellationToken);
        }
                
        public async Task<(int, IEnumerable<TModel>)> GetAsync(Filter<TModel> filter, CancellationToken cancellationToken)
        {
            return await Execute<(int, IEnumerable<TModel>)>(async (token) => {
                var all = context.Set<TModel>().Where(filter.Selector);

                if (!string.IsNullOrEmpty(filter.Sort))
                {
                    all = all.OrderBy(filter.Sort);
                }
                var count = await all.CountAsync();
                if (filter.Page != null && filter.Size != null)
                {
                    all = all.Skip(filter.Page.Value * filter.Size.Value).Take(filter.Size.Value);
                }

                return (count, await all.ToListAsync(cancellationToken));
            }, "GetAsync", cancellationToken);
        }

        private async Task<TResult> Execute<TResult>(Func<CancellationToken, Task<TResult>> action, string methodName, CancellationToken cancellationToken)
        {
            try
            {
                return await action(cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError($"Error in {methodName} method: {ex.Message} \r\nStackTrace: {ex.StackTrace}");
                throw new DatabaseException($"Error in {methodName} method: {ex.Message}");
            }
        }

        public async Task SaveChangesAsync()
        {
            await context.SaveChangesAsync();
        }
    }
}
