using Microsoft.Extensions.Logging;
using ProjectBranchSelector.DbClient.Context;
using ProjectBranchSelector.DbClient.Interface;
using ProjectBranchSelector.DbClient.Model.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace ProjectBranchSelector.DbClient.Repository
{
    public class Repository<TModel> : IRepository<TModel> where TModel : Entity
    {
        private readonly DbSqLiteContext _context;
        private readonly ILogger<Repository<TModel>> _logger;

        public Repository(DbSqLiteContext context, ILogger<Repository<TModel>> logger)
        {
            _context = context;
            _logger = logger;
        }

        public IQueryable<TModel> GetAll(bool withDeleted = false)
        {
            var result = _context.Set<TModel>().AsQueryable();
            if (!withDeleted) result = result.Where(s => !s.IsDeleted);
            return result;
        }

        public TModel Get(Guid id, bool withDeleted = false)
        {
            return Execute(() => {
                var res = _context.Set<TModel>().AsQueryable();
                if (!withDeleted) res = res.Where(s => !s.IsDeleted);
                return res.Where(s => s.Id == id).FirstOrDefault();
            }, "GetAsync");          
        }

        public TModel Add(TModel entity, bool withSave = false)
        {
            return Execute(() => {
                var result = _context.Set<TModel>().Add(entity).Entity;
                if (withSave)
                {
                    _context.SaveChanges();
                }
                return result;
            }, "AddAsync");
        }

        public TModel Update(TModel entity, bool withSave = false)
        {
            return Execute( () => {
                var result = _context.Set<TModel>().Update(entity).Entity;
                if (withSave)
                {
                   _context.SaveChanges();
                }
                return result;
            }, "UpdateAsync");           
        }

        public TModel Remove(Guid id, bool withSave = false)
        {
            return Execute(() => {
                var toRemove =  _context.Set<TModel>()
                    .Where(s => s.Id == id).FirstOrDefault();
                toRemove.IsDeleted = true;
                var result = _context.Set<TModel>().Update(toRemove).Entity;
                if (withSave)
                {
                    _context.SaveChanges();
                }
                return result;
            }, "RemoveAsync");
        }

        public IEnumerable<TModel> Get(Filter<TModel> filter, bool withDeleted = false)
        {
            return Execute(()=> {
                var all = _context.Set<TModel>().AsQueryable();

                if (!withDeleted) all = all.Where(s => !s.IsDeleted);
                all = all.Where(filter.Selector);
                if (!string.IsNullOrEmpty(filter.Sort))
                {
                   // all = all.OrderBy(filter.Sort);
                }

                all = all.Skip(filter.Page * filter.Size).Take(filter.Size);

                return all.ToList();
            }, "GetAsync");            
        }

        private TResult Execute<TResult>(Func<TResult> action, string methodName)
        {
            try
            {
                return action();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in {methodName} method: {ex.Message} \r\nStackTrace: {ex.StackTrace}");
                throw new DatabaseException($"Error in {methodName} method: {ex.Message}");
            }
        }

    }

    [Serializable]
    public class DatabaseException : Exception
    {
        public DatabaseException()
        {
        }

        public DatabaseException(string message) : base(message)
        {
        }

        public DatabaseException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected DatabaseException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
