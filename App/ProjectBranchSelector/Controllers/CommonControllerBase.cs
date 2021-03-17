using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ProjectBranchSelector.Models;
using ProjectBranchSelector.Service;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ProjectBranchSelector.Controllers
{
    public abstract class CommonApiControllerBase<T, TH> : CommonControllerBase 
        where T: EntityModel
        where TH: EntityHistoryModel
    {
        public CommonApiControllerBase(IServiceProvider serviceProvider) : base(serviceProvider) { }

        protected async Task<IActionResult> GetEntities<TFilter>(Func<TFilter, CancellationToken, Task<(int, IEnumerable<T>)>> getAction,
            TFilter filter, string methodName) where TFilter: Filter<T>
        {
            return await GetEntitiesInternal(getAction, filter, methodName);
        }        

        protected async Task<IActionResult> GetHistory<TFilter>(Func<TFilter, CancellationToken, Task<(int, IEnumerable<TH>)>> getAction,
            TFilter filter, string methodName) where TFilter : Filter<TH>
        {
            return await GetEntitiesInternal(getAction, filter, methodName);            
        }

        protected async Task<IActionResult> GetChanges(Func<ChangesFilter, CancellationToken, Task<IEnumerable<TH>>> getAction, long lastHId, int size,
            string methodName)
        {
            return await GetChangesInternal(getAction, lastHId, size, methodName);
        }

        protected async Task<IActionResult> GetEntity(Func<Guid, CancellationToken, Task<T>> action, Guid id, string methodName)
        {
            try
            {
                return Ok(await action(id, cancellationToken));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Method {methodName} exception: {ex.Message} + ST: {ex.StackTrace}");
                return InternalServerError($"Method {methodName} exception: {ex.Message}");
            }
        }        

        protected async Task<IActionResult> ModifyItem<TCreator>(Func<TCreator, CancellationToken, Task<T>> action, TCreator item, string methodName)
        {
            return await ModifyItemInternal(action, item, methodName);            
        }        
    }

    public abstract class CommonControllerBase : Controller
    {
        protected ILogger _logger;
        protected IDataService dataService;
        protected CancellationToken cancellationToken;
        protected CancellationTokenSource tokenSource;

        public CommonControllerBase(IServiceProvider serviceProvider)
        {           
            dataService = serviceProvider.GetRequiredService<IDataService>();
            tokenSource = new CancellationTokenSource();
            cancellationToken = tokenSource.Token;
        }

        protected InternalServerErrorObjectResult InternalServerError()
        {
            return new InternalServerErrorObjectResult();
        }

        protected InternalServerErrorObjectResult InternalServerError(object value)
        {
            return new InternalServerErrorObjectResult(value);
        }

        protected async Task<IActionResult> ModifyItemInternal<Req, Resp>(Func<Req, CancellationToken, Task<Resp>> action, Req item, string methodName)
        {
            try
            {
                return Ok(await action(item, cancellationToken));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Method {methodName} exception: {ex.Message} + ST: {ex.StackTrace}");
                return InternalServerError($"Method {methodName} exception: {ex.Message}");
            }
        }

        protected async Task<IActionResult> GetEntitiesInternal<TI, TFilter>(Func<TFilter, CancellationToken, Task<(int, IEnumerable<TI>)>> getAction,
            TFilter filter, string methodName)
            where TI : EntityModel
            where TFilter : Filter<TI>
        {
            return await GetEntitiesInternalCommon((ct) => getAction(filter, ct), methodName);
        }

        protected async Task<IActionResult> GetEntitiesInternalCommon<TI>(Func<CancellationToken, Task<(int, IEnumerable<TI>)>> getAction, string methodName)
            where TI : EntityModel
        {
            try
            {
                var result = await getAction(cancellationToken);
                if (Response != null) Response.Headers.Add("x-pages", result.Item1.ToString());
                return Ok(result.Item2);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Method {methodName} exception: {ex.Message} + ST: {ex.StackTrace}");
                return InternalServerError($"Method {methodName} exception: {ex.Message}");
            }
        }

        protected async Task<IActionResult> GetChangesInternal<TI>(Func<ChangesFilter, CancellationToken, Task<IEnumerable<TI>>> getAction,
            long lastHId, int size, string methodName)
        {
            try
            {
                return Ok(await getAction(new ChangesFilter(lastHId, size), cancellationToken));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Method {methodName} exception: {ex.Message} + ST: {ex.StackTrace}");
                return InternalServerError($"Method {methodName} exception: {ex.Message}");
            }
        }
    }
}