using AutoMapper;
using ProjectBranchSelector.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ProjectBranchSelector.Models;
using ProjectBranchSelector.Db.Interface;
using ProjectBranchSelector.Db.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using FormulaCalc;
using Newtonsoft.Json.Linq;

namespace ProjectBranchSelector.Service
{
    public class DataService : IDataService
    {
        
        private readonly IRepository<Tree> repositoryTree;
        private readonly IRepository<Formula> repositoryFormula;
        private readonly IRepository<TreeItem> repositoryTreeItem;
        private readonly IRepositoryHistory<TreeHistory> repositoryTreeHistory;
        private readonly IRepositoryHistory<TreeItemHistory> repositoryTreeItemHistory;
        private readonly IRepositoryHistory<FormulaHistory> repositoryFormulaHistory;
        private readonly ILogger _logger;
        private readonly IMapper mapper;
        private readonly ICalculator calculator;

        public DataService(IServiceProvider serviceProvider)
        {
            repositoryTree = serviceProvider.GetRequiredService<IRepository<Tree>>();
            repositoryTreeHistory = serviceProvider.GetRequiredService<IRepositoryHistory<TreeHistory>>();
            repositoryTreeItemHistory = serviceProvider.GetRequiredService<IRepositoryHistory<TreeItemHistory>>();
            repositoryTreeItem = serviceProvider.GetRequiredService<IRepository<TreeItem>>();
            repositoryFormula = serviceProvider.GetRequiredService<IRepository<Formula>>();
            repositoryFormulaHistory = serviceProvider.GetRequiredService<IRepositoryHistory<FormulaHistory>>();
            mapper = serviceProvider.GetRequiredService<IMapper>();
            _logger = serviceProvider.GetRequiredService<ILogger<DataService>>();
            calculator = serviceProvider.GetRequiredService<ICalculator>();
        }

        public async Task<(int, IEnumerable<TreeModel>)> GetTrees(TreeFilter filter, CancellationToken cancellationToken)
        {
            try
            {
                var all = (await repositoryTree.GetAsync(new Db.Interface.Filter<Tree>()
                {
                    Page = filter.Page,
                    Size = filter.Size,
                    Sort = filter.Sort,
                    Selector = s => string.IsNullOrEmpty(filter.Name) || s.Name.ToLower().Contains(filter.Name.ToLower())
                }, cancellationToken));
                var result = all.Item2.Select(s => mapper.Map<TreeModel>(s)).ToList();
                if (filter.WithExtension)
                {
                    var ids = result.Select(c => c.FormulaId).ToList();
                    var formulas = (await repositoryFormula.GetAsync(new Db.Interface.Filter<Formula>() {
                        Selector = s => ids.Contains(s.Id)
                    }, cancellationToken)).Item2.ToList();
                    foreach (var item in result)
                    {
                        item.Formula = formulas.FirstOrDefault(s => s.Id == item.FormulaId).Name;
                    }
                }
                return (all.Item1, result);
            }
            catch (RepositoryException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while execute GetTrees: {ex.Message} StackTrace: {ex.StackTrace}");
                throw new DataServiceException(ex.Message);
            }
        }

        public async Task<TreeModel> GetTree(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                var result = await repositoryTree.GetAsync(id, cancellationToken);
                if (result != null)
                    return mapper.Map<TreeModel>(result);
                throw new DataServiceException($"Дерево c id = {id} не найдено в базе данных");
            }
            catch (RepositoryException)
            {
                throw;
            }
            catch (DataServiceException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while execute GetTree: {ex.Message} StackTrace: {ex.StackTrace}");
                throw new DataServiceException(ex.Message);
            }
        }

        public async Task<TreeModel> AddTree(TreeCreator item, CancellationToken cancellationToken)
        {
            try
            {
                var check = await repositoryTree.GetAsync(new Db.Interface.Filter<Tree>()
                {
                    Selector = s => s.Name == item.Name
                }, cancellationToken);
                if (check.Item2.Any())
                    throw new DataServiceException($"Дерево с наименованием {item.Name} уже существует");

                var result = await repositoryTree.AddAsync(new Tree()
                {
                    Description = item.Description,
                    FormulaId = item.FormulaId,
                    Id = Guid.NewGuid(),
                    Name = item.Name
                }, cancellationToken, true);
                if (result != null)
                    return mapper.Map<TreeModel>(result);
                throw new DataServiceException($"Не удалось добавить дерево с наименованием {item.Name}"); ;
            }
            catch (RepositoryException)
            {
                throw;
            }
            catch (DataServiceException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while AddTree execute: {ex.Message}; StackTrace: {ex.StackTrace}");
            }
            return null;
        }

        public async Task<TreeModel> UpdateTree(TreeUpdater item, CancellationToken cancellationToken)
        {
            try
            {

                var entity = await repositoryTree.GetAsync(item.Id, cancellationToken);
                if (entity == null)
                    throw new RepositoryException($"Tree with {item.Id} not exists");

                var checkName = await repositoryTree.GetAsync(new Db.Interface.Filter<Tree>()
                {
                    Selector = s => s.Name == item.Name && s.Id != item.Id
                }, cancellationToken);

                if (checkName.Item2.Any())
                    throw new RepositoryException($"Дерево с наименованием {item.Name} уже существует");

                entity.Description = item.Description;
                entity.Name = item.Name;
                entity.FormulaId = item.FormulaId;

                var result = await repositoryTree.UpdateAsync(entity, cancellationToken, true);
                if (result != null)
                    return mapper.Map<TreeModel>(result);
            }
            catch (RepositoryException)
            {
                throw;
            }
            catch (DataServiceException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while UpdateTree execute: {ex.Message}; StackTrace: {ex.StackTrace}");
                throw new DataServiceException($"Error while UpdateTree execute: {ex.Message};");
            }
            return null;
        }

        public async Task<TreeModel> DeleteTree(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                var entity = await repositoryTree.RemoveAsync(id, cancellationToken, false);
                if (entity != null)
                {
                    var items = await repositoryTreeItem.GetAsync(new Db.Interface.Filter<TreeItem>()
                    {
                        Selector = s => s.TreeId == id
                    }, cancellationToken);
                    foreach (var item in items.Item2)
                    {
                        await repositoryTreeItem.RemoveAsync(item.Id, cancellationToken, false);
                    }
                    await repositoryTreeItem.SaveChangesAsync();
                    return mapper.Map<TreeModel>(entity);
                }
                throw new DataServiceException($"Дерево c id = {id} не найдено в базе данных");
            }
            catch (RepositoryException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while execute DeleteTree: {ex.Message} StackTrace: {ex.StackTrace}");
                throw new DataServiceException(ex.Message);
            }
        }

        public async Task<(int, IEnumerable<TreeHistoryModel>)> GetTreeHistory(TreeHistoryFilter treeHistoryFilter, CancellationToken cancellationToken)
        {
            try
            {
                var all = (await repositoryTreeHistory.GetAsync(new Db.Interface.Filter<TreeHistory>()
                {
                    Page = treeHistoryFilter.Page,
                    Size = treeHistoryFilter.Size,
                    Sort = treeHistoryFilter.Sort,
                    Selector = s =>
                        (treeHistoryFilter.Name == null || s.Name.ToLower().Contains(treeHistoryFilter.Name.ToLower())) &&
                        (treeHistoryFilter.Id == null || s.Id == treeHistoryFilter.Id) &&
                        (treeHistoryFilter.From == null || s.ChangeDate >= treeHistoryFilter.From) &&
                        (treeHistoryFilter.To == null || s.ChangeDate <= treeHistoryFilter.To)
                }, cancellationToken));
                return (all.Item1, all.Item2.Select(s => mapper.Map<TreeHistoryModel>(s)));
            }
            catch (RepositoryException)
            {
                throw;
            }
            catch (DataServiceException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while execute GetTreeHistory: {ex.Message} StackTrace: {ex.StackTrace}");
                throw new DataServiceException(ex.Message);
            }
        }

        public async Task<(int, IEnumerable<FormulaModel>)> GetFormulas(FormulaFilter filter, CancellationToken cancellationToken)
        {
            try
            {
                var all = (await repositoryFormula.GetAsync(new Db.Interface.Filter<Formula>()
                {
                    Page = filter.Page,
                    Size = filter.Size,
                    Sort = filter.Sort,
                    Selector = s => string.IsNullOrEmpty(filter.Name) || s.Name.ToLower().Contains(filter.Name.ToLower())
                }, cancellationToken));
                return (all.Item1, all.Item2.Select(s => mapper.Map<FormulaModel>(s)));
            }
            catch (RepositoryException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while execute GetFormulas: {ex.Message} StackTrace: {ex.StackTrace}");
                throw new DataServiceException(ex.Message);
            }
        }

        public async Task<FormulaModel> GetFormula(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                var result = await repositoryFormula.GetAsync(id, cancellationToken);
                if (result != null)
                    return mapper.Map<FormulaModel>(result);
                throw new DataServiceException($"Формула c id = {id} не найдена в базе данных");
            }
            catch (RepositoryException)
            {
                throw;
            }
            catch (DataServiceException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while GetFormula execute: {ex.Message}; StackTrace: {ex.StackTrace}");
                throw new DataServiceException(ex.Message);
            }
        }

        public async Task<FormulaModel> AddFormula(FormulaCreator item, CancellationToken cancellationToken)
        {
            try
            {
                var check = await repositoryFormula.GetAsync(new Db.Interface.Filter<Formula>()
                {
                    Selector = s => s.Name == item.Name
                }, cancellationToken);
                if (check.Item2.Any())
                    throw new RepositoryException($"Формула с наименованием {item.Name} уже существует");

                if (item.IsDefault)
                {
                    var curDefaults = await repositoryFormula.GetAsync(new Db.Interface.Filter<Formula>()
                    {
                        Selector = s => s.IsDefault
                    }, cancellationToken);
                    foreach (var defItem in curDefaults.Item2)
                    {
                        defItem.IsDefault = false;
                        await repositoryFormula.UpdateAsync(defItem, cancellationToken, false);
                    }
                }

                var id = Guid.NewGuid();
                var result = await repositoryFormula.AddAsync(new Formula()
                {
                    Id = id,
                    Name = item.Name,
                    IsDefault = item.IsDefault,
                    Text = item.Text
                }, cancellationToken, false);

                await repositoryFormula.SaveChangesAsync();

                if (result != null)
                    return mapper.Map<FormulaModel>(result);
                throw new DataServiceException($"Ошибка при добавлении формулы: пустой результат");
            }
            catch (RepositoryException)
            {
                throw;
            }
            catch (DataServiceException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка при добавлении формулы: {ex.Message}; StackTrace: {ex.StackTrace}");
                throw new DataServiceException($"Ошибка при добавлении формулы: {ex.Message}");
            }            
        }

        public async Task<FormulaModel> UpdateFormula(FormulaUpdater item, CancellationToken cancellationToken)
        {
            try
            {
                var currentItem = await repositoryFormula.GetAsync(item.Id, cancellationToken);
                if (currentItem == null)
                    throw new RepositoryException($"Formula not exists");

                var checkName = await repositoryFormula.GetAsync(new Db.Interface.Filter<Formula>()
                {
                    Selector = s => s.Name == item.Name && s.Id != item.Id
                }, cancellationToken);

                if (checkName.Item2.Any())
                    throw new RepositoryException($"Formula with {item.Name} already exists");

                if (item.IsDefault)
                {
                    var curDefaults = await repositoryFormula.GetAsync(new Db.Interface.Filter<Formula>()
                    {
                        Selector = s => s.IsDefault
                    }, cancellationToken);
                    foreach (var defItem in curDefaults.Item2)
                    {
                        defItem.IsDefault = false;
                        await repositoryFormula.UpdateAsync(defItem, cancellationToken, false);
                    }
                }
                currentItem.Name = item.Name;
                currentItem.IsDefault = item.IsDefault;
                currentItem.Text = item.Text;

                var result = await repositoryFormula
                    .UpdateAsync(currentItem, cancellationToken, false);

                await repositoryFormula.SaveChangesAsync();

                if (result != null)
                    return mapper.Map<FormulaModel>(result);
                throw new DataServiceException($"Unknown error while UpdateFormula execute, formula not updated");
            }
            catch (RepositoryException)
            {
                throw;
            }
            catch (DataServiceException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while UpdateFormula execute: {ex.Message}; StackTrace: {ex.StackTrace}");
                throw new DataServiceException($"Error while UpdateFormula execute: {ex.Message};");
            }
        }

        public async Task<FormulaModel> DeleteFormula(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                var entity = await repositoryFormula.RemoveAsync(id, cancellationToken, true);
                if (entity != null)
                {    
                    return mapper.Map<FormulaModel>(entity);
                }
                throw new DataServiceException($"Формула c id = {id} не найдена в базе данных");
            }
            catch (DataServiceException)
            {
                throw;
            }
            catch (RepositoryException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while execute DeleteFormula: {ex.Message} StackTrace: {ex.StackTrace}");
                throw new DataServiceException(ex.Message);
            }
        }

        public async Task<(int, IEnumerable<FormulaHistoryModel>)> GetFormulaHistory(FormulaHistoryFilter filter, CancellationToken cancellationToken)
        {
            try
            {
                var all = (await repositoryFormulaHistory.GetAsync(new Db.Interface.Filter<FormulaHistory>()
                {
                    Page = filter.Page,
                    Size = filter.Size,
                    Sort = filter.Sort,
                    Selector = s =>
                        (filter.Name == null || s.Name.ToLower().Contains(filter.Name.ToLower())) &&
                        (filter.Id == null || s.Id == filter.Id) &&
                        (filter.From == null || s.ChangeDate >= filter.From) &&
                        (filter.To == null || s.ChangeDate <= filter.To)
                }, cancellationToken));
                return (all.Item1, all.Item2.Select(s => mapper.Map<FormulaHistoryModel>(s)));
            }
            catch (RepositoryException)
            {
                throw;
            }
            catch (DataServiceException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while execute GetFormulaHistory: {ex.Message} StackTrace: {ex.StackTrace}");
                throw new DataServiceException(ex.Message);
            }
        }


        public async Task<TreeItemModel> GetTreeItem(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                var result = await repositoryTreeItem.GetAsync(id, cancellationToken);
                if(result!=null)
                    return mapper.Map<TreeItemModel>(result);
                throw new DataServiceException($"Элемент дерева c id = {id} не найден в базе данных");
            }
            catch (RepositoryException)
            {
                throw;
            }
            catch (DataServiceException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while GetTreeItem execute: {ex.Message}; StackTrace: {ex.StackTrace}");
                throw new DataServiceException(ex.Message);
            }            
        }
                
        public async Task<(int, IEnumerable<TreeItemModel>)> GetTreeItems(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                var filter = new Db.Interface.Filter<TreeItem>()
                {                    
                    Selector = s=>s.TreeId == id
                };
                var result = await repositoryTreeItem.GetAsync(filter, cancellationToken);
                return (result.Item1, result.Item2.Select(s => mapper.Map<TreeItemModel>(s)));
            }
            catch (RepositoryException)
            {
                throw;
            }
            catch (DataServiceException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while GetTreeItems execute: {ex.Message}; StackTrace: {ex.StackTrace}");
                throw new DataServiceException($"Error while GetTreeItems execute: {ex.Message}");
            }            
        }
               
        public async Task<IEnumerable<TreeItemModel>> AddTreeItems(TreeItemsUpdater request, CancellationToken cancellationToken)
        {
            try
            {
                var existsTree = await repositoryTreeItem.GetAll()
                    .Where(s => s.TreeId == request.TreeId).ToListAsync();

                var oldIds = existsTree.Select(c => c.Id).ToList();
                var newIds = new List<Guid>();
                Check(request.TreeItems, "");
                foreach (var item in request.TreeItems)
                {
                    newIds.AddRange(await AddBranch(item, null, existsTree, request.TreeId, "", cancellationToken));
                }

                var toDelete = existsTree.Where(s => !newIds.Contains(s.Id));
                foreach (var item in toDelete)
                {
                    await repositoryTreeItem.RemoveAsync(item.Id, cancellationToken, false);
                }
                await repositoryTreeItem.SaveChangesAsync();
                var result = await repositoryTreeItem.GetAll()
                    .Where(s => s.TreeId == request.TreeId).ToListAsync();

                return result.Select(s=>mapper.Map<TreeItemModel>(s));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while AddTree execute: {ex.Message}; StackTrace: {ex.StackTrace}");
            }
            return null;
        }

        public async Task<(int, IEnumerable<TreeItemHistoryModel>)> GetTreeItemsHistory(TreeItemsHistoryFilter treeItemsHistoryFilter, CancellationToken cancellationToken)
        {
            try
            {
                var all = (await repositoryTreeItemHistory.GetAsync(new Db.Interface.Filter<TreeItemHistory>()
                {
                    Page = treeItemsHistoryFilter.Page,
                    Size = treeItemsHistoryFilter.Size,
                    Sort = treeItemsHistoryFilter.Sort,
                    Selector = s =>
                        (treeItemsHistoryFilter.Name == null || s.Name.ToLower().Contains(treeItemsHistoryFilter.Name.ToLower())) &&
                        (treeItemsHistoryFilter.TreeId == null || s.TreeId == treeItemsHistoryFilter.TreeId) &&
                        (treeItemsHistoryFilter.From == null || s.ChangeDate >= treeItemsHistoryFilter.From) &&
                        (treeItemsHistoryFilter.To == null || s.ChangeDate <= treeItemsHistoryFilter.To)
                }, cancellationToken));
                return (all.Item1, all.Item2.Select(s => mapper.Map<TreeItemHistoryModel>(s)));
            }
            catch (RepositoryException)
            {
                throw;
            }
            catch (DataServiceException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while execute GetTreeItemsHistory: {ex.Message} StackTrace: {ex.StackTrace}");
                throw new DataServiceException(ex.Message);
            }
        }

        public async Task<TreeItemModel> AddTreeItem(TreeItemCreator item, CancellationToken cancellationToken)
        {
            try
            {
                var check = await repositoryTreeItem.GetAsync(new Db.Interface.Filter<TreeItem>()
                {
                    Selector = s => s.Name == item.Name
                       && s.TreeId == item.TreeId
                       && s.ParentId == item.ParentId
                }, cancellationToken);
                if (check.Item2.Any())
                    throw new RepositoryException($"TreeItem with {item.Name} already exists");

                var id = Guid.NewGuid();
                var result = await repositoryTreeItem.AddAsync(new TreeItem()
                {
                    Id = id,
                    Name = item.Name,
                    Description = item.Description,
                    AddFields = item.AddFields,
                    IsLeaf = true,
                    ParentId = item.ParentId,
                    SelectCount = 0,
                    TreeId = item.TreeId,
                    Weight = item.Weight
                }, cancellationToken, false);

                if (item.ParentId.HasValue)
                {
                    var parent = await repositoryTreeItem.GetAsync(item.ParentId.Value, cancellationToken);
                    parent.IsLeaf = false;
                    await repositoryTreeItem.UpdateAsync(parent, cancellationToken, false);
                }

                await repositoryTreeItem.SaveChangesAsync();

                if (result != null)
                    return mapper.Map<TreeItemModel>(result);
                throw new DataServiceException("Unknown error while AddTreeItem execute");
            }
            catch (RepositoryException)
            {
                throw;
            }
            catch (DataServiceException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while AddTreeItem execute: {ex.Message}; StackTrace: {ex.StackTrace}");
                throw new DataServiceException($"Error while AddTreeItem execute: {ex.Message}");
            }
        }

        public async Task<TreeItemModel> UpdateTreeItem(TreeItemUpdater item, CancellationToken cancellationToken)
        {
            try
            {
                var entity = await repositoryTreeItem.GetAsync(item.Id.Value, cancellationToken);
                if (entity == null)
                    throw new RepositoryException($"TreeItem with {item.Name} not exists");

                if ((item.ParentId.HasValue || entity.ParentId.HasValue) && item.ParentId != entity.ParentId)
                {
                    if (item.ParentId.HasValue)
                    {
                        var parent = await repositoryTreeItem.GetAsync(item.ParentId.Value, cancellationToken);
                        parent.IsLeaf = false;
                        await repositoryTreeItem.UpdateAsync(parent, cancellationToken, false);
                    }
                    if (entity.ParentId.HasValue)
                    {
                        var parent = await repositoryTreeItem.GetAsync(item.ParentId.Value, cancellationToken);
                        var check = await repositoryTreeItem.GetAsync(new Db.Interface.Filter<TreeItem>() { 
                          Selector = s=>s.TreeId == parent.TreeId 
                                && s.ParentId == parent.Id
                        }, cancellationToken);
                        if (!check.Item2.Any())
                        {
                            parent.IsLeaf = true;
                            await repositoryTreeItem.UpdateAsync(parent, cancellationToken, false);
                        }
                    }
                }
                entity.AddFields = item.AddFields;
                entity.Name = item.Name;
                entity.Description = item.Description;
                entity.ParentId = item.ParentId;
                entity.Weight = item.Weight;
                var result = await repositoryTreeItem.UpdateAsync(entity, cancellationToken, false);

                await repositoryTreeItem.SaveChangesAsync();

                if (result != null)
                    return mapper.Map<TreeItemModel>(result);
                throw new DataServiceException("Unknown error while update TreeItem");
            }
            catch (RepositoryException)
            {
                throw;
            }
            catch (DataServiceException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while UpdateTreeItem execute: {ex.Message}; StackTrace: {ex.StackTrace}");
                throw new DataServiceException($"Error while UpdateTreeItem execute: {ex.Message}");
            }            
        }

        public async Task<TreeItemModel> DeleteTreeItem(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                var entity = await repositoryTreeItem.RemoveAsync(id, cancellationToken, true);
                if (entity != null)
                {
                    if(entity.ParentId.HasValue)
                    {                        
                        var parent = await repositoryTreeItem.GetAsync(entity.ParentId.Value, cancellationToken);
                        var check = await repositoryTreeItem.GetAsync(new Db.Interface.Filter<TreeItem>()
                        {
                            Selector = s => s.TreeId == parent.TreeId
                                  && s.ParentId == parent.Id
                        }, cancellationToken);
                        if (!check.Item2.Any())
                        {
                            parent.IsLeaf = true;
                            await repositoryTreeItem.UpdateAsync(parent, cancellationToken, true);
                        }
                    }
                    return mapper.Map<TreeItemModel>(entity);
                }
                throw new DataServiceException($"Элемент c id = {id} не найден в базе данных");
            }
            catch (DataServiceException)
            {
                throw;
            }
            catch (RepositoryException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while execute DeleteTreeItem: {ex.Message} StackTrace: {ex.StackTrace}");
                throw new DataServiceException(ex.Message);
            }
        }

        public async Task<SelectResponse> SelectItem(SelectRequest item, CancellationToken cancellationToken)
        {
            try
            {
                var tree = await repositoryTree.GetAsync(item.TreeId, cancellationToken);
                var formula = await repositoryFormula.GetAsync(tree.FormulaId, cancellationToken);
                var treeItems = await repositoryTreeItem.GetAsync(new Db.Interface.Filter<TreeItem>()
                {
                    Selector = s => s.TreeId == item.TreeId
                }, cancellationToken);
                List<CalcRequestItem> items = new List<CalcRequestItem>();
                foreach (var treeItem in treeItems.Item2)
                {
                    if (item.LeafOnly && !treeItem.IsLeaf) continue;
                    CalcRequestItem calcRequestItem = new CalcRequestItem
                    {
                        Id = treeItem.Id
                    };
                    JObject fields = new JObject
                    {
                        { "SelectCount", treeItem.SelectCount },
                        { "Weight", treeItem.Weight }
                    };
                    var addFields = JObject.Parse(treeItem.AddFields);
                    foreach (var addField in addFields)
                    {
                        fields.Add(addField);
                    }
                    calcRequestItem.Fields = fields.ToString();
                    items.Add(calcRequestItem);
                }
                List<SelectResponseElement> responseElements = new List<SelectResponseElement>();
                if (items?.Any() == true)
                {
                    var result = calculator.Calculate(new CalcRequest()
                    {
                        ChangeOnSelect = s =>
                        {
                            var item = items.FirstOrDefault(c => c.Id == s);
                            var fields = JObject.Parse(item.Fields);
                            fields["SelectCount"] = int.Parse(fields["SelectCount"].ToString()) + 1;
                            item.Fields = fields.ToString();
                        },
                        Count = item.Count,
                        Formula = formula.Text,
                        Items = items
                    });

                    
                    var itemsChanged = treeItems.Item2.Where(s => result.Select(c => c.Id).Contains(s.Id));
                    foreach (var res in result)
                    {
                        var treeItem = itemsChanged.FirstOrDefault(s => s.Id == res.Id);
                        SelectResponseElement element = new SelectResponseElement
                        {
                            Id = res.Id,
                            Name = treeItem.Name,
                            FullPath = GetElementFullPath(res.Id, treeItems.Item2, tree)
                        };
                        treeItem.SelectCount++;
                        responseElements.Add(element);
                    }
                    foreach (var itemChanged in itemsChanged)
                    {
                        await repositoryTreeItem.UpdateAsync(itemChanged, cancellationToken);
                    }
                    await repositoryTreeItem.SaveChangesAsync();                    
                }
                return new SelectResponse()
                {
                    Result = responseElements
                };
            }
            catch (RepositoryException)
            {
                throw;
            }
            catch (DataServiceException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while execute SelectItem: {ex.Message} StackTrace: {ex.StackTrace}");
                throw new DataServiceException(ex.Message);
            }
        }

        public SelectResponse SelectItemCustom(SelectRequestCustom item, CancellationToken cancellationToken)
        {
            try
            {                
                List<CalcRequestItem> items = new List<CalcRequestItem>();
                foreach (var treeItem in item.Tree)
                {
                    CalcRequestItem calcRequestItem = new CalcRequestItem
                    {
                        Id = treeItem.Id,
                        Fields = treeItem.Fields
                    };                    
                }

                var result = calculator.Calculate(new CalcRequest()
                {                   
                    Count = item.Count,
                    Formula = item.Formula,
                    Items = items,
                });

                List<SelectResponseElement> responseElements = new List<SelectResponseElement>();
                var itemsChanged = item.Tree.Where(s => result.Select(c => c.Id).Contains(s.Id));
                foreach (var res in result)
                {
                    var treeItem = itemsChanged.FirstOrDefault(s => s.Id == res.Id);
                    SelectResponseElement element = new SelectResponseElement
                    {
                        Id = res.Id,
                        Name = treeItem.Name,
                        FullPath = GetElementFullPath(res.Id, item.Tree)
                    };                    
                }                
                return new SelectResponse()
                {
                    Result = responseElements
                };

            }
            catch (RepositoryException)
            {
                throw;
            }
            catch (DataServiceException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while execute SelectItem: {ex.Message} StackTrace: {ex.StackTrace}");
                throw new DataServiceException(ex.Message);
            }
        }

        private async Task<List<Guid>> AddBranch(TreeItemUpdater treeItem, Guid? parentId, 
            List<TreeItem> oldItems, Guid treeId, string parentPath, CancellationToken cancellationToken)
        {
            List<Guid> result = new List<Guid>();
            
            bool isLeaf = (treeItem.TreeItems==null || !treeItem.TreeItems.Any());
            var id = treeItem.Id ?? Helper.GenerateGuid(new string[] { treeItem.Name, (parentId ?? Guid.Empty).ToString() });
            result.Add(id);
            if (!oldItems.Select(s=>s.Id).Contains(id))
            {
                await repositoryTreeItem.AddAsync(new TreeItem()
                {
                    Id = id,
                    AddFields = treeItem.AddFields,                    
                    IsLeaf = isLeaf,
                    Name = treeItem.Name,
                    Description = treeItem.Description,
                    ParentId = parentId,
                    TreeId = treeId,                    
                    Weight = treeItem.Weight
                }, cancellationToken, false);
            }
            else
            {
                var item = oldItems.FirstOrDefault(s => s.Id == id);
                item.AddFields = treeItem.AddFields;
                item.IsLeaf = isLeaf;
                item.Name = treeItem.Name;
                item.Description = treeItem.Description;
                item.ParentId = parentId;                
                item.Weight = treeItem.Weight;
                await repositoryTreeItem.UpdateAsync(item, cancellationToken, false);
            }

            if (!isLeaf)
            {
                Check(treeItem.TreeItems, $"{parentPath}/{treeItem.Name}");
                foreach (var item in treeItem.TreeItems)
                {
                    result.AddRange(await AddBranch(item, id, oldItems, treeId, $"{parentPath}/{treeItem.Name}", cancellationToken));
                }
            }

            return result;
        }

        private void Check(IEnumerable<TreeItemUpdater> treeItems, string parentPath)
        {
            var check = treeItems.ToList()
                .GroupBy(s => s.Name).Where(s=>s.Count()>0);

            if (check.Count() > 0)
            {
                throw new Exception($"Error in check data in AddTreeItems: double Name and ParentId in:" +
                    $" {string.Join(", ", check.Select(s => $"Path: {parentPath}/{s.FirstOrDefault().Name}"))}");
            }
        }
                
        private string[] GetElementFullPath(Guid id, IEnumerable<TreeItem> treeItems, Tree tree)
        {
            List<string> elements = new List<string>();
            var treeItem = treeItems.FirstOrDefault(s => s.Id == id);
            elements.Add(treeItem.Name);
            while (treeItem.ParentId != null)
            {
                treeItem = treeItems.FirstOrDefault(s => s.Id == treeItem.ParentId);
                elements.Add(treeItem.Name);
            }
            elements.Add(tree.Name);
            elements.Reverse();
            return elements.ToArray();
        }

        private string[] GetElementFullPath(Guid id, IEnumerable<TreeItemCustom> treeItems)
        {
            List<string> elements = new List<string>();
            var treeItem = treeItems.FirstOrDefault(s => s.Id == id);
            elements.Add(treeItem.Name);
            while (treeItem.ParentId != null)
            {
                treeItem = treeItems.FirstOrDefault(s => s.Id == treeItem.ParentId);
                elements.Add(treeItem.Name);
            }            
            elements.Reverse();
            return elements.ToArray();
        }

        public async Task<int> GetTreesCount(TreeFilter filter, CancellationToken cancellationToken)
        {
            try
            {
                var result = await repositoryTree.GetAll()
                    .Where(s => string.IsNullOrEmpty(filter.Name) || s.Name.ToLower().Contains(filter.Name.ToLower())).CountAsync();

                if (filter.Size.HasValue)
                {
                    var size = filter.Size.Value;
                    if (result % size == 0) return result / size;
                    return (result / size) + 1;
                }
                return result;
            }
            catch (RepositoryException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while execute GetTreesCount: {ex.Message} StackTrace: {ex.StackTrace}");
                throw new DataServiceException(ex.Message);
            }
        }

        public async Task<IEnumerable<TreeHistoryModel>> GetTreeChanges(ChangesFilter treeChangesFilter, CancellationToken cancellationToken)
        {
            try
            {
                var all = (await repositoryTreeHistory.GetAsync(new Db.Interface.Filter<TreeHistory>()
                {
                    Page = 0,
                    Size = treeChangesFilter.Size,
                    Sort = "HId",
                    Selector = s => s.HId > treeChangesFilter.LastHid
                }, cancellationToken));
                return all.Item2.Select(s => mapper.Map<TreeHistoryModel>(s));
            }
            catch (RepositoryException)
            {
                throw;
            }
            catch (DataServiceException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while execute GetTreeChanges: {ex.Message} StackTrace: {ex.StackTrace}");
                throw new DataServiceException(ex.Message);
            }
        }

        public async Task<IEnumerable<TreeItemHistoryModel>> GetTreeItemsChanges(ChangesFilter changesFilter, CancellationToken cancellationToken)
        {
            try
            {
                var all = (await repositoryTreeItemHistory.GetAsync(new Db.Interface.Filter<TreeItemHistory>()
                {
                    Page = 0,
                    Size = changesFilter.Size,
                    Sort = "HId",
                    Selector = s => s.HId > changesFilter.LastHid
                }, cancellationToken));
                return all.Item2.Select(s => mapper.Map<TreeItemHistoryModel>(s));
            }
            catch (RepositoryException)
            {
                throw;
            }
            catch (DataServiceException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while execute GetTreeItemsChanges: {ex.Message} StackTrace: {ex.StackTrace}");
                throw new DataServiceException(ex.Message);
            }
        }

        public async Task<IEnumerable<FormulaHistoryModel>> GetFormulaChanges(ChangesFilter changesFilter, CancellationToken cancellationToken)
        {
            try
            {
                var all = (await repositoryFormulaHistory.GetAsync(new Db.Interface.Filter<FormulaHistory>()
                {
                    Page = 0,
                    Size = changesFilter.Size,
                    Sort = "HId",
                    Selector = s => s.HId > changesFilter.LastHid
                }, cancellationToken));
                return all.Item2.Select(s => mapper.Map<FormulaHistoryModel>(s));
            }
            catch (RepositoryException)
            {
                throw;
            }
            catch (DataServiceException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while execute GetFormulaChanges: {ex.Message} StackTrace: {ex.StackTrace}");
                throw new DataServiceException(ex.Message);
            }
        }

        public async Task<FormulaModel> GetDefaultFormula(CancellationToken cancellationToken)
        {
            try
            {
                var result = await repositoryFormula.GetAsync(new Db.Interface.Filter<Formula>()
                { 
                    Selector = s=>s.IsDefault
                }, cancellationToken);
                if (result.Item2.Any())
                    return mapper.Map<FormulaModel>(result.Item2.FirstOrDefault());
                throw new DataServiceException($"Формула по умолчанию не найдена в базе данных");
            }
            catch (RepositoryException)
            {
                throw;
            }
            catch (DataServiceException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while GetDefaultFormula execute: {ex.Message}; StackTrace: {ex.StackTrace}");
                throw new DataServiceException(ex.Message);
            }
        }
    }

    [Serializable]
    internal class DataServiceException : Exception
    {
        public DataServiceException()
        {
        }

        public DataServiceException(string message) : base(message)
        {
        }

        public DataServiceException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected DataServiceException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
