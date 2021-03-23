using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ProjectBranchSelector.BSHttpClient;
using ProjectBranchSelector.Common;
using ProjectBranchSelector.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectBranchSelector.DesktopApp.Service
{
    public class DataService : IDataService
    {
        private readonly IBSHttpClient _httpClient;
        private readonly IDbService _dbService;
        private readonly ILogger<DataService> _logger;
        private ClientMode clientMode;

        public DataService(IBSHttpClient httpClient, IDbService dbService, ILogger<DataService> logger, IOptions<CommonOptions> options)
        {
            _httpClient = httpClient;
            _dbService = dbService;
            _logger = logger;
            clientMode = options.Value.ClientMode;
        }

        public async Task<(int, IEnumerable<TreeModel>)> GetTrees(string name, int? page, int? size, string sort)
        {
            return await Execute(
                () => ExecuteSafeAsync(()=>_httpClient.Get<TreeModel>(
                    CreateParamString(
                        new Dictionary<string, string>() {
                            { nameof(name), name }, 
                            { nameof(page), page?.ToString()??"" }, 
                            { nameof(size), size?.ToString()??"" }, 
                            { nameof(sort), sort }
                    })), (0, null), "GetTrees"),
                () => ExecuteSafe(
                    ()=> _dbService.GetTrees(name, page, size, sort), 
                    (0, null), 
                    "GetTrees"), 
                (0, null), s=>s.Item2!=null);
        }

        internal Task<SelectResponse> SelectItem(SelectRequest selectRequest)
        {
            throw new NotImplementedException();
        }

        public async Task<(int, IEnumerable<TreeItemModel>)> GetTreeItems(Guid id)
        {
            return await Execute(
                ()=>ExecuteSafeAsync(()=> _httpClient.Get<TreeItemModel>($"{id}/items", typeof(TreeModel)), (0, null), "GetTreeItems"), 
                ()=>ExecuteSafe(()=> _dbService.GetTreeItems(id), (0, null), "GetTreeItems"),
                (0, null), s => s.Item2 != null
            );
        }
               
        public async Task<FormulaModel> GetFormula(Guid id)
        {
            return await Execute(
                () => ExecuteSafeAsync(() => _httpClient.Get<FormulaModel>(id), null, "GetFormula"),
                () => ExecuteSafe(() => _dbService.GetFormula(id), null, "GetFormula"),
                null, s => s != null
            );          
        }

        public async Task<TreeModel> GetTree(Guid id)
        {
            return await Execute(
                () => ExecuteSafeAsync(() => _httpClient.Get<TreeModel>(id), null, "GetTree"),
                () => ExecuteSafe(() => _dbService.GetTree(id), null, "GetTree"),
                null, s => s != null
            );           
        }

        public async Task<(int, IEnumerable<FormulaModel>)> GetFormulas(string name, int? page, int? size, string sort)
        {
            return await Execute(
                () => ExecuteSafeAsync(() => _httpClient.Get<FormulaModel>(
                    CreateParamString(
                        new Dictionary<string, string>() {
                            { nameof(name), name },
                            { nameof(page), page?.ToString()??"" },
                            { nameof(size), size?.ToString()??"" },
                            { nameof(sort), sort }
                    })), (0, null), "GetFormulas"),
                () => ExecuteSafe(
                    () => _dbService.GetFormulas(name, page, size, sort),
                    (0, null),
                    "GetFormulas"),
                (0, null), s => s.Item2 != null);            
        }

        public async Task<Guid?> GetDefaultFormula()
        {
            var result = await Execute(
                async () => (await ExecuteSafeAsync(() => _httpClient.Get<FormulaModel>(
                    CreateParamString(
                        new Dictionary<string, string>() {
                            { "isDefault", "true" }
                    })), (0, null), "GetDefaultFormula")).Item2?.FirstOrDefault(),
                () => ExecuteSafe(
                    () => _dbService.GetDefaultFormula(),
                    null,
                    "GetDefaultFormula"),
                null, s => s != null);
            return result.Id;
        }

        public async Task<TreeItemModel> GetTreeItem(Guid id)
        {
            return await Execute(
                () => ExecuteSafeAsync(() => _httpClient.Get<TreeItemModel>(id), null, "GetTreeItem"),
                () => ExecuteSafe(() => _dbService.GetTreeItem(id), null, "GetTreeItem"),
                null, s => s != null
            );           
        }

        public async Task<TreeModel> AddTree(TreeCreator tree)
        {
            return await ExecuteModifyAsync(
                (r,d)=> _dbService.AddTreeSyncronized(r,d), 
                t=> _dbService.AddTreeNotSyncronized(t), 
                t=> ExecuteSafeAsync(()=> _httpClient.Post<TreeCreator, TreeModel>(t), null, "AddTree"),
                tree
            );            
        }                

        public async Task<TreeModel> UpdateTree(TreeUpdater tree)
        {
            return await ExecuteModifyAsync(
                (r, d) => _dbService.UpdateTreeSyncronized(r, d),
                t => _dbService.UpdateTreeNotSyncronized(t),
                t => ExecuteSafeAsync(() => _httpClient.Put<TreeUpdater, TreeModel>(t), null, "UpdateTree"),
                tree
            );
        }

        public async Task<TreeModel> DeleteTree(Guid id)
        {
            return await ExecuteModifyAsync(
                (r, d) => _dbService.DeleteTreeSyncronized(r.Id, d),
                t => _dbService.DeleteTreeNotSyncronized(t),
                t => ExecuteSafeAsync(() => _httpClient.Delete<TreeModel>(t), null, "DeleteTree"),
                id
            );
        }

        public async Task<TreeItemModel> AddTreeItem(TreeItemCreator item)
        {
            return await ExecuteModifyAsync(
               (r, d) => _dbService.AddTreeItemSyncronized(r, d),
               t => _dbService.AddTreeItemNotSyncronized(t),
               t => ExecuteSafeAsync(() => _httpClient.Post<TreeItemCreator, TreeItemModel>(t), null, "AddTreeItem"),
               item
           );
        }

        public async Task<TreeItemModel> UpdateTreeItem(TreeItemUpdater item)
        {
            return await ExecuteModifyAsync(
               (r, d) => _dbService.UpdateTreeItemSyncronized(r, d),
               t => _dbService.UpdateTreeItemNotSyncronized(t),
               t => ExecuteSafeAsync(() => _httpClient.Put<TreeItemUpdater, TreeItemModel>(t), null, "UpdateTreeItem"),
               item
           );            
        }

        public async Task<FormulaModel> AddFormula(FormulaCreator formulaCreator)
        {
            return await ExecuteModifyAsync(
                (r, d) => _dbService.AddFormulaSyncronized(r, d),
                t => _dbService.AddFormulaNotSyncronized(t),
                t => ExecuteSafeAsync(() => _httpClient.Post<FormulaCreator, FormulaModel>(t), null, "AddFormula"),
                formulaCreator
            );
        }

        public async Task<TreeItemModel> DeleteTreeItem(Guid id)
        {
            return await ExecuteModifyAsync(
                (r, d) => _dbService.DeleteTreeItemSyncronized(r.Id, d),
                t => _dbService.DeleteTreeItemNotSyncronized(t),
                t => ExecuteSafeAsync(() => _httpClient.Delete<TreeItemModel>(t), null, "DeleteTreeItem"),
                id
            );
        }

        private string CreateParamString(Dictionary<string, string> args)
        {
            List<string> paramItems = new List<string>();
            string param = "";
            foreach (var par in args)
            {
                if (!string.IsNullOrEmpty(par.Value))
                {
                    paramItems.Add($"{par.Key}={par.Value}");
                }
            }
            if (paramItems.Any())
            {
                param = $"?{string.Join("&", paramItems)}";
            }
            return param;
        }

        private async Task<T> Execute<T>(Func<Task<T>> thinExecute, Func<T> thickExecute, T defaultValue, Func<T, bool> checkResult)
        {
            T result = defaultValue;
            if (clientMode == ClientMode.ThickOnly || clientMode == ClientMode.ThickPriority)
            {
                result = thickExecute();
            }
            if (clientMode == ClientMode.ThinOnly || clientMode == ClientMode.ThinPriority)
            {
                result = await thinExecute();
            }
            if (checkResult(result)) return result;
            if (clientMode == ClientMode.ThickPriority)
            {
                result = await thinExecute();
            }
            if (clientMode == ClientMode.ThinPriority)
            {
                result = thickExecute();
            }
            return result;
        }

        private T ExecuteSafe<T>(Func<T> execute, T defaultValue, string voidName)
        {
            try
            {
                return execute();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while {voidName} from dataBase: {ex.Message}; StackTrace {ex.StackTrace}");
            }
            return defaultValue;
        }

        private async Task<T> ExecuteSafeAsync<T>(Func<Task<T>> execute, T defaultValue, string voidName)
        {
            if (_httpClient.IsConnected)
            {
                try
                {
                    return await execute();
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error while {voidName} from httpClient: {ex.Message}; StackTrace {ex.StackTrace}");
                }
            }
            return defaultValue;
        }

        private async Task<T> ExecuteModifyAsync<T, F>(Func<T, DateTimeOffset, T> execThickSync,
            Func<F, T> execThickNotSync, Func<F, Task<T>> execThin, F arg)
            where T : class
        {
            T result = null;
            if (clientMode != ClientMode.ThickOnly)
            {
                result = await execThin(arg);
            }
            if (clientMode == ClientMode.ThinOnly) return result;
            if (result != null)
            {
                return execThickSync(result, DateTimeOffset.Now);
            }
            return execThickNotSync(arg);
        }

        public async Task<FormulaModel> UpdateFormula(FormulaUpdater formulaUpdater)
        {
            return await ExecuteModifyAsync(
                (r, d) => _dbService.UpdateFormulaSyncronized(r, d),
                t => _dbService.UpdateFormulaNotSyncronized(t),
                t => ExecuteSafeAsync(() => _httpClient.Post<FormulaUpdater, FormulaModel>(t), null, "UpdateFormula"),
                formulaUpdater
            );
        }
    }
}
