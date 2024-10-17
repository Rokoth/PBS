using Microsoft.Extensions.Logging;
using ProjectBranchSelector.BSHttpClient;
using ProjectBranchSelector.DbClient;
using ProjectBranchSelector.DbClient.Context;
using ProjectBranchSelector.DbClient.Interface;
using ProjectBranchSelector.DesktopApp.Service;
using ProjectBranchSelector.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DesktopApp.Service
{
    public class SyncService : ISyncService, IDisposable
    {
        private readonly IBSHttpClient<DataHttpClientSettings> _httpClient;
        private readonly IDbService _dbService;
        private readonly ILogger<DataService> _logger;
        private readonly IRepository<Tree> _repositoryTree;
        private readonly IRepository<TreeItem> _repositoryTreeItem;
        private readonly IRepository<Formula> _repositoryFormula;
        private bool isDisposed = false;
        private bool nowSync = false;

        public event EventHandler OnSync;

        public SyncService(IBSHttpClient<DataHttpClientSettings> httpClient, IDbService dbService, ILogger<DataService> logger
            , IRepository<Tree> repositoryTree, IRepository<Formula> repositoryFormula, IRepository<TreeItem> repositoryTreeItem)
        {
            _httpClient = httpClient;
            _dbService = dbService;
            _logger = logger;
            _repositoryTree = repositoryTree;
            _repositoryFormula = repositoryFormula;
            _repositoryTreeItem = repositoryTreeItem;
            httpClient.OnConnect += HttpClient_OnConnect; ;
        }

        private async void HttpClient_OnConnect(object sender, EventArgs e)
        {
            await DoSync();
        }

        public void Start()
        {
            Task.Factory.StartNew(DoWork, TaskCreationOptions.LongRunning);
        }

        private async Task DoWork()
        {
            while (!isDisposed)
            {
                await DoSync();
                await Task.Delay(60000);
            }
        }

        private async Task DoSync()
        {
            if (_httpClient.IsConnected && !nowSync)
            {
                nowSync = true;
                try
                {
                    var syncFormulas = await SyncFormulas();
                    var syncTrees = await SyncTrees();
                    var syncItems = await SyncTreeItems();
                    if (syncFormulas || syncTrees || syncItems)
                    {
                        OnSync?.Invoke(this, new EventArgs());
                    }
                    _logger.LogError($"Sync executed normal");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error while sync: {ex.Message} {ex.StackTrace}");
                }
                nowSync = false;
            }
        }

        private async Task<bool> SyncTrees()
        {
            bool uploaded = false;
            long lastHid = 0;
            var lasthIdString = _dbService.GetSettings("TreeLastHId");
            if (!string.IsNullOrEmpty(lasthIdString))
                lastHid = long.Parse(lasthIdString);
            var treeChanges = await _httpClient.Get<TreeHistoryModel>($"?lastHid={lastHid}&size=100");
            if (treeChanges.Item2 != null && treeChanges.Item2.Any())
            {
                uploaded = true;
                while (treeChanges.Item2.Any())
                {
                    foreach (var tree in treeChanges.Item2.OrderBy(s => s.HId))
                    {
                        var existsTree = _repositoryTree.Get(tree.Id, true);
                        if (existsTree == null) existsTree = _repositoryTree.GetAll(true).Where(s => s.Name == tree.Name).FirstOrDefault();
                        if (existsTree == null)
                        {
                            _repositoryTree.Add(new Tree()
                            {
                                Description = tree.Description,
                                FormulaId = tree.FormulaId,
                                Id = tree.Id,
                                IsDeleted = tree.IsDeleted,
                                IsSync = true,
                                Name = tree.Name,
                                VersionDate = tree.ChangeDate
                            }, true);
                        }
                        else if (existsTree.VersionDate < tree.ChangeDate)
                        {
                            existsTree.Description = tree.Description;
                            existsTree.FormulaId = tree.FormulaId;
                            existsTree.Id = tree.Id;
                            existsTree.IsDeleted = tree.IsDeleted;
                            existsTree.Name = tree.Name;
                            existsTree.VersionDate = tree.ChangeDate;
                            existsTree.IsSync = true;
                            _repositoryTree.Update(existsTree, true);
                        }
                    }
                    lastHid = treeChanges.Item2.Max(s => s.HId);
                    treeChanges = await _httpClient.Get<TreeHistoryModel>($"?lastHid={lastHid}&size=100");
                }
                _dbService.SaveSettings("TreeLastHId", lastHid.ToString());
            }
            var notSyncTrees = _repositoryTree.GetAll(true).Where(s => !s.IsSync);
            foreach (var tree in notSyncTrees)
            {
                TreeModel res = null;
                var check = await _httpClient.Get<TreeModel>(tree.Id);
                if (check == null) check = (await _httpClient.Get<TreeModel>($"?name={tree.Name}")).Item2.FirstOrDefault();                
                if (check == null)
                {
                    res = await _httpClient.Post<TreeCreator, TreeModel>(new TreeCreator()
                    {
                        Description = tree.Description,
                        FormulaId = tree.FormulaId,
                        Name = tree.Name
                    });
                }
                else
                {
                    res = await _httpClient.Put<TreeUpdater, TreeModel>(new TreeUpdater()
                    {
                        Description = tree.Description,
                        FormulaId = tree.FormulaId,
                        Id = tree.Id,
                        Name = tree.Name
                    });
                }               
                _dbService.UpdateTreeSyncronized(res, DateTimeOffset.Now);
            }
            return uploaded;
        }

        private async Task<bool> SyncFormulas()
        {
            bool uploaded = false;
            long lastHid = 0;
            var lasthIdString = _dbService.GetSettings("FormulaLastHId");
            if (!string.IsNullOrEmpty(lasthIdString))
                lastHid = long.Parse(lasthIdString);
            var changes = await _httpClient.Get<FormulaHistoryModel>($"?lastHid={lastHid}&size=100");
            if (changes.Item2 != null && changes.Item2.Any())
            {
                uploaded = true;
                while (changes.Item2.Any())
                {
                    foreach (var item in changes.Item2.OrderBy(s => s.HId))
                    {
                        var exists = _repositoryFormula.Get(item.Id, true);
                        if (exists == null) exists = _repositoryFormula.GetAll(true).Where(s => s.Name == item.Name).FirstOrDefault();
                        if (exists == null)
                        {
                            _repositoryFormula.Add(new Formula()
                            {
                                Id = item.Id,
                                IsDeleted = item.IsDeleted,
                                IsSync = true,
                                Name = item.Name,
                                VersionDate = item.ChangeDate,
                                IsDefault = item.IsDefault,
                                Text = item.Text
                            }, true);
                        }
                        else if (exists.VersionDate < item.ChangeDate)
                        {
                            exists.IsDefault = item.IsDefault;
                            exists.Text = item.Text;
                            exists.Id = item.Id;
                            exists.IsDeleted = item.IsDeleted;
                            exists.Name = item.Name;
                            exists.VersionDate = item.ChangeDate;
                            exists.IsSync = true;
                            _repositoryFormula.Update(exists, true);
                        }
                    }
                    lastHid = changes.Item2.Max(s => s.HId);
                    changes = await _httpClient.Get<FormulaHistoryModel>($"?lastHid={lastHid}&size=100");
                }
                _dbService.SaveSettings("FormulaLastHId", lastHid.ToString());
            }
            var notSync = _repositoryFormula.GetAll(true).Where(s => !s.IsSync);
            foreach (var item in notSync)
            {
                FormulaModel res = null;
                var check = await _httpClient.Get<FormulaModel>(item.Id);
                if (check == null) check = (await _httpClient.Get<FormulaModel>($"?name={item.Name}")).Item2.FirstOrDefault();
                if (check == null)
                {
                    res = await _httpClient.Post<FormulaCreator, FormulaModel>(new FormulaCreator()
                    {
                        Name = item.Name,
                        Text = item.Text,
                        IsDefault = item.IsDefault
                    });
                }
                else
                {
                    res = await _httpClient.Put<FormulaUpdater, FormulaModel>(new FormulaUpdater()
                    {
                        Name = item.Name,
                        Text = item.Text,
                        IsDefault = item.IsDefault,
                        Id = item.Id
                    });
                }
                res = _dbService.UpdateFormulaSyncronized(res, DateTimeOffset.Now);
            }
            return uploaded;
        }

        private async Task<bool> SyncTreeItems()
        {
            var uploaded = false;
            long lastHid = 0;
            var lasthIdString = _dbService.GetSettings("TreeItemLastHId");
            if (!string.IsNullOrEmpty(lasthIdString))
                lastHid = long.Parse(lasthIdString);
            var changes = await _httpClient.Get<TreeItemHistoryModel>($"?lastHid={lastHid}&size=100");
            if (changes.Item2 != null && changes.Item2.Any())
            {
                uploaded = true;
                while (changes.Item2.Any())
                {
                    foreach (var item in changes.Item2.OrderBy(s => s.HId))
                    {
                        var exists = _repositoryTreeItem.Get(item.Id, true);
                        if (exists == null) exists = _repositoryTreeItem.GetAll(true).Where(s => s.Name == item.Name).FirstOrDefault();
                        if (exists == null)
                        {
                            _repositoryTreeItem.Add(new TreeItem()
                            {
                                Id = item.Id,
                                IsDeleted = item.IsDeleted,
                                IsSync = true,
                                Name = item.Name,
                                Description = item.Description,
                                VersionDate = item.ChangeDate,
                                AddFields = item.AddFields,
                                IsLeaf = item.IsLeaf,
                                ParentId = item.ParentId,
                                SelectCount = item.SelectCount,
                                TreeId = item.TreeId,
                                Weight = item.Weight
                            }, true);
                        }
                        else if (exists.VersionDate < item.ChangeDate)
                        {
                            exists.AddFields = item.AddFields;
                            exists.IsLeaf = item.IsLeaf;
                            exists.ParentId = item.ParentId;
                            exists.SelectCount = item.SelectCount;
                            exists.TreeId = item.TreeId;
                            exists.Weight = item.Weight;                            
                            exists.IsDeleted = item.IsDeleted;
                            exists.Name = item.Name;
                            exists.Description = item.Description;
                            exists.VersionDate = item.ChangeDate;
                            exists.IsSync = true;
                            _repositoryTreeItem.Update(exists, true);
                        }
                    }
                    lastHid = changes.Item2.Max(s => s.HId);
                    changes = await _httpClient.Get<TreeItemHistoryModel>($"?lastHid={lastHid}&size=100");
                }
                _dbService.SaveSettings("TreeItemLastHId", lastHid.ToString());
            }
            var notSync = _repositoryTreeItem.GetAll(true).Where(s => !s.IsSync);
            foreach (var item in notSync)
            {
                TreeItemModel res = null;
                var check = await _httpClient.Get<TreeItemModel>(item.Id);
                if (check == null) check = (await _httpClient.Get<TreeItemModel>(
                    $"?treeId={item.TreeId}&parentId={item.ParentId}&name={item.Name}")).Item2.FirstOrDefault();
                if (check == null)
                {
                    res = await _httpClient.Post<TreeItemCreator, TreeItemModel>(new TreeItemCreator()
                    {
                        Name = item.Name,
                        Description = item.Description,
                        AddFields = item.AddFields,
                        ParentId = item.ParentId,
                        SelectCount = item.SelectCount,
                        TreeId = item.TreeId,
                        Weight = item.Weight
                    });
                }
                else
                {
                    res = await _httpClient.Put<TreeItemUpdater, TreeItemModel>(new TreeItemUpdater()
                    {
                        Name = item.Name,
                        Description = item.Description,
                        AddFields = item.AddFields,
                        ParentId = item.ParentId,
                        SelectCount = item.SelectCount,
                        TreeId = item.TreeId,
                        Weight = item.Weight,
                        Id = item.Id
                    });
                }
                _dbService.UpdateTreeItemSyncronized(res, DateTimeOffset.Now);                
            }
            return uploaded;
        }

        public void Dispose()
        {
            isDisposed = true;
        }
    }
}
