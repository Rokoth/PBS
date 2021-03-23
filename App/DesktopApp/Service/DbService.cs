using AutoMapper;
using DesktopApp.Service;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ProjectBranchSelector.DbClient;
using ProjectBranchSelector.DbClient.Context;
using ProjectBranchSelector.DbClient.Interface;
using ProjectBranchSelector.DbClient.Repository;
using ProjectBranchSelector.DesktopApp.Interface;
using ProjectBranchSelector.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ProjectBranchSelector.DesktopApp.Service
{
    public class DbService : IDbService
    {
        private readonly IRepository<Tree> repositoryTree;
        private readonly IRepository<TreeItem> repositoryTreeItem;
        private readonly IRepository<Formula> repositoryFormula;
        private readonly ILogger<DbService> logger;
        private readonly IMapper mapper;
        private readonly IServiceProvider _serviceProvider;

        public DbService(IServiceProvider serviceProvider)
        {
            repositoryTree = serviceProvider.GetRequiredService<IRepository<Tree>>();
            repositoryTreeItem = serviceProvider.GetRequiredService<IRepository<TreeItem>>();
            repositoryFormula = serviceProvider.GetRequiredService<IRepository<Formula>>();
            logger = serviceProvider.GetRequiredService<ILogger<DbService>>();
            mapper = serviceProvider.GetRequiredService<IMapper>();
            _serviceProvider = serviceProvider;
        }

        public TreeModel AddTreeNotSyncronized(TreeCreator tree)
        {
            try
            {
                var id = Guid.NewGuid();
                var check = repositoryTree.GetAll().FirstOrDefault(s => s.Name == tree.Name);
                if (check != null)
                    throw new DataServiceException($"Дерево с наименованием {tree.Name} уже существует");

                var result = repositoryTree.Add(new Tree()
                { 
                      Description = tree.Description,
                      FormulaId = tree.FormulaId,
                      Id = id,
                      Name = tree.Name,
                      IsSync = false,
                      VersionDate = DateTimeOffset.Now
                }, true);
                if (result != null)
                    return mapper.Map<TreeModel>(result);
                throw new DataServiceException($"Ошибка при добавлении дерева {tree.Name}");
            }
            catch (DatabaseException)
            {
                throw;
            }
            catch (DataServiceException)
            {
                throw;
            }
            catch (Exception ex)
            {
                logger.LogError($"Ошибка при добавлении дерева {tree.Name}: {ex.Message} {ex.StackTrace}");
                throw new DataServiceException($"Ошибка при добавлении дерева {tree.Name}: {ex.Message}");
            }
        }

        public TreeModel AddTreeSyncronized(TreeModel tree, DateTimeOffset versionDate)
        {
            try
            {                
                var check = repositoryTree.GetAll().FirstOrDefault(s => s.Name == tree.Name);
                if (check != null)
                    throw new DataServiceException($"Дерево с наименованием {tree.Name} уже существует");

                var result = repositoryTree.Add(new Tree()
                {
                    Description = tree.Description,
                    FormulaId = tree.FormulaId,
                    Id = tree.Id,
                    Name = tree.Name,
                    IsSync = true,
                    VersionDate = versionDate
                }, true);
                if (result != null)
                    return mapper.Map<TreeModel>(result);
                throw new DataServiceException($"Ошибка при добавлении дерева {tree.Name}");
            }
            catch (DatabaseException)
            {
                throw;
            }
            catch (DataServiceException)
            {
                throw;
            }
            catch (Exception ex)
            {
                logger.LogError($"Ошибка при добавлении дерева {tree.Name}: {ex.Message} {ex.StackTrace}");
                throw new DataServiceException($"Ошибка при добавлении дерева {tree.Name}: {ex.Message}");
            }
        }

        public FormulaModel GetFormula(Guid id)
        {
            try
            {
                var item = repositoryFormula.Get(id);
                if(item!=null)
                    return mapper.Map<FormulaModel>(item);
                throw new DataServiceException();
            }
            catch (DatabaseException)
            {
                throw;
            }
            catch (DataServiceException)
            {
                throw;
            }
            catch (Exception ex)
            {
                logger.LogError($"Ошибка получения формул: {ex.Message} {ex.StackTrace}");
                throw new DataServiceException($"Ошибка при получении формулы: {ex.Message}");
            }
        }

        public (int, IEnumerable<FormulaModel>) GetFormulas(string name, int? page, int? size, string sort)
        {
            try
            {
                var all = repositoryFormula.GetAll()
                    .Where(s =>
                       (string.IsNullOrEmpty(name) || s.Name == name));

                if (!string.IsNullOrEmpty(sort))
                {
                    if (sort.Equals("name", StringComparison.InvariantCultureIgnoreCase))
                    {
                        all = all.OrderBy(s => s.Name);
                    }
                }
                var count = all.Count();
                if (page.HasValue && size.HasValue)
                {
                    all = all.Skip(page.Value * size.Value).Take(size.Value);
                }
                return (count, all.ToList().Select(s => mapper.Map<FormulaModel>(s)));
            }
            catch (DatabaseException)
            {
                throw;
            }
            catch (Exception ex)
            {
                logger.LogError($"Ошибка получения формул: {ex.Message} {ex.StackTrace}");
                throw new DataServiceException($"Ошибка при получении формул: {ex.Message}");
            }
        }

        

        public TreeModel GetTree(Guid id)
        {
            try
            {
                var item = repositoryTree.Get(id);
                if (item != null)
                    return mapper.Map<TreeModel>(item);
                throw new DataServiceException("Формула не найдена в базе");
            }
            catch (DatabaseException)
            {
                throw;
            }
            catch (DataServiceException)
            {
                throw;
            }
            catch (Exception ex)
            {
                logger.LogError($"Ошибка получения дерева: {ex.Message} {ex.StackTrace}");
                throw new DataServiceException($"Ошибка получения дерева: {ex.Message}");
            }
        }

        public TreeItemModel GetTreeItem(Guid id)
        {
            try
            {
                var item = repositoryTreeItem.Get(id);
                if (item != null)
                    return mapper.Map<TreeItemModel>(item);
                throw new DataServiceException("Элемент дерева не найден в базе");
            }
            catch (DatabaseException)
            {
                throw;
            }
            catch (DataServiceException)
            {
                throw;
            }
            catch (Exception ex)
            {
                logger.LogError($"Ошибка получения формул: {ex.Message} {ex.StackTrace}");
                throw new DataServiceException($"Ошибка при получении формулы: {ex.Message}");
            }
        }

        public (int, IEnumerable<TreeItemModel>) GetTreeItems(Guid id)
        {
            try
            {
                var all = repositoryTreeItem.GetAll()
                    .Where(s =>s.TreeId == id);
                
                return (all.Count(), all.ToList().Select(s => mapper.Map<TreeItemModel>(s)));
            }
            catch (DatabaseException)
            {
                throw;
            }
            catch (Exception ex)
            {
                logger.LogError($"Ошибка получения элементов дерева: {ex.Message} {ex.StackTrace}");
                throw new DataServiceException($"Ошибка получения элементов дерева: {ex.Message}");
            }
        }

        public (int, IEnumerable<TreeModel>) GetTrees(string name, int? page, int? size, string sort)
        {
            try
            {
                var all = repositoryTree.GetAll()
                    .Where(s=>(string.IsNullOrEmpty(name) || s.Name.ToLower().Contains(name.ToLower())))
                    .Join(repositoryFormula.GetAll(), s=>s.FormulaId, s=>s.Id, (t,f)=>new Tree()
                    { 
                       Description = t.Description,
                       Formula = f,
                       FormulaId = t.FormulaId,
                       Id = t.Id,
                       IsDeleted = t.IsDeleted,
                       IsSync = t.IsSync,
                       Name = t.Name,
                       VersionDate = t.VersionDate
                    });

                if (!string.IsNullOrEmpty(sort))
                {
                    if (sort.Equals("name", StringComparison.InvariantCultureIgnoreCase))
                    {
                        all = all.OrderBy(s=>s.Name);
                    }                    
                }
                var count = all.Count();
                if (page.HasValue && size.HasValue)
                {
                    all = all.Skip(page.Value * size.Value).Take(size.Value);
                }
                return (count, all.ToList().Select(s => mapper.Map<TreeModel>(s)));
            }
            catch (DatabaseException)
            {
                throw;
            }
            catch (Exception ex)
            {
                logger.LogError($"Ошибка получения списка деревьев: {ex.Message} {ex.StackTrace}");
                throw new DataServiceException($"Ошибка получения списка деревьев: {ex.Message}");
            }
        }

        public string GetSettings(string key)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var provider = scope.ServiceProvider;
                var context = provider.GetRequiredService<DbSqLiteContext>();
                var setting = context.Set<Settings>().FirstOrDefault(s => s.ParamName == key);
                if (setting != null)
                {
                    return setting.ParamValue;                    
                }
                return null;
            }
        }

        public void SaveSettings(string key, string value)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var provider = scope.ServiceProvider;
                var context = provider.GetRequiredService<DbSqLiteContext>();
                var setting = context.Set<Settings>().FirstOrDefault(s => s.ParamName == key);
                if (setting != null)
                {
                    setting.ParamValue = value;
                    context.Update(setting);
                }
                else
                {
                    int maxId = 0;
                    if (context.Set<Settings>().Any())
                    {
                        maxId = context.Set<Settings>().Max(s => s.Id);
                    }
                    setting = new Settings()
                    {
                        Id = maxId + 1,
                        ParamName = key,
                        ParamValue = value
                    };
                    context.Add(setting);
                }
                context.SaveChanges();
            }
        }

        public TreeModel UpdateTreeNotSyncronized(TreeUpdater tree)
        {
            try
            {               
                var entity = repositoryTree.Get(tree.Id);
                if (entity == null)
                    throw new DataServiceException($"Дерево с id={tree.Id} не существует");

                entity.Description = tree.Description;
                entity.FormulaId = tree.FormulaId;
                entity.IsSync = false;
                entity.Name = tree.Name;
                entity.VersionDate = DateTimeOffset.Now;

                var result = repositoryTree.Update(entity, true);
                if (result != null)
                    return mapper.Map<TreeModel>(result);
                throw new DataServiceException($"Ошибка при обновлении дерева {tree.Name}");
            }
            catch (DatabaseException)
            {
                throw;
            }
            catch (DataServiceException)
            {
                throw;
            }
            catch (Exception ex)
            {
                logger.LogError($"Ошибка при обновлении дерева {tree.Name}: {ex.Message} {ex.StackTrace}");
                throw new DataServiceException($"Ошибка при обновлении дерева {tree.Name}: {ex.Message}");
            }
        }

        public TreeModel UpdateTreeSyncronized(TreeModel tree, DateTimeOffset versionDate)
        {
            try
            {
                var entity = repositoryTree.Get(tree.Id);
                if (entity == null)
                    throw new DataServiceException($"Дерево с id={tree.Id} не существует");

                entity.Description = tree.Description;
                entity.FormulaId = tree.FormulaId;
                entity.IsSync = true;
                entity.Name = tree.Name;
                entity.VersionDate = versionDate;

                var result = repositoryTree.Update(entity, true);
                if (result != null)
                    return mapper.Map<TreeModel>(result);
                throw new DataServiceException($"Ошибка при обновлении дерева {tree.Name}");
            }
            catch (DatabaseException)
            {
                throw;
            }
            catch (DataServiceException)
            {
                throw;
            }
            catch (Exception ex)
            {
                logger.LogError($"Ошибка при обновлении дерева {tree.Name}: {ex.Message} {ex.StackTrace}");
                throw new DataServiceException($"Ошибка при обновлении дерева {tree.Name}: {ex.Message}");
            }
        }

        public TreeItemModel UpdateTreeItemSyncronized(TreeItemModel treeItem, DateTimeOffset versionDate)
        {
            try
            {
                var entity = repositoryTreeItem.Get(treeItem.Id);
                if (entity == null)
                    throw new DataServiceException($"Элемент с id={treeItem.Id} не существует");

                entity.Description = treeItem.Description;
                entity.IsLeaf = treeItem.IsLeaf;
                entity.TreeId = treeItem.TreeId;
                entity.AddFields = treeItem.AddFields;
                entity.Name = treeItem.Name;
                entity.ParentId = treeItem.ParentId;
                entity.SelectCount = treeItem.SelectCount;
                entity.Weight = treeItem.Weight;               
                entity.IsSync = true;                
                entity.VersionDate = versionDate;

                var result = repositoryTreeItem.Update(entity, true);
                if (result != null)
                    return mapper.Map<TreeItemModel>(result);
                throw new DataServiceException($"Ошибка при обновлении элемента дерева {treeItem.Name}");
            }
            catch (DatabaseException)
            {
                throw;
            }
            catch (DataServiceException)
            {
                throw;
            }
            catch (Exception ex)
            {
                logger.LogError($"Ошибка при обновлении элемента дерева {treeItem.Name}: {ex.Message} {ex.StackTrace}");
                throw new DataServiceException($"Ошибка при обновлении элемента дерева {treeItem.Name}: {ex.Message}");
            }
        }

        public TreeItemModel UpdateTreeItemNotSyncronized(TreeItemUpdater treeItem)
        {
            try
            {                
                var entity = repositoryTreeItem.Get(treeItem.Id.Value);
                if (entity == null)
                    throw new DataServiceException($"Элемент с id={treeItem.Id} не существует");

                entity.Description = treeItem.Description;
                var childs = repositoryTreeItem.GetAll().Where(s => s.ParentId == treeItem.Id.Value);
                entity.IsLeaf = !childs.Any();
                entity.TreeId = treeItem.TreeId.Value;
                entity.AddFields = treeItem.AddFields;
                entity.Name = treeItem.Name;
                entity.ParentId = treeItem.ParentId;
                entity.SelectCount = treeItem.SelectCount;
                entity.Weight = treeItem.Weight;
                entity.IsSync = false;
                entity.VersionDate = DateTimeOffset.Now;

                var result = repositoryTreeItem.Update(entity, true);
                if (result != null)
                    return mapper.Map<TreeItemModel>(result);
                throw new DataServiceException($"Ошибка при обновлении элемента дерева {treeItem.Name}");
            }
            catch (DatabaseException)
            {
                throw;
            }
            catch (DataServiceException)
            {
                throw;
            }
            catch (Exception ex)
            {
                logger.LogError($"Ошибка при обновлении элемента дерева {treeItem.Name}: {ex.Message} {ex.StackTrace}");
                throw new DataServiceException($"Ошибка при обновлении элемента дерева {treeItem.Name}: {ex.Message}");
            }
        }

        public TreeModel DeleteTreeSyncronized(Guid id, DateTimeOffset now)
        {
            try
            {
                var entity = repositoryTree.Get(id);
                if (entity == null)
                    throw new DataServiceException($"Дерево с id={id} не существует");

                entity.IsDeleted = true;                
                entity.IsSync = true;               
                entity.VersionDate = now;

                var result = repositoryTree.Update(entity, true);
                if (result != null)
                    return mapper.Map<TreeModel>(result);
                throw new DataServiceException($"Ошибка при удалении дерева {entity.Name}");
            }
            catch (DatabaseException)
            {
                throw;
            }
            catch (DataServiceException)
            {
                throw;
            }
            catch (Exception ex)
            {
                logger.LogError($"Ошибка при удалении дерева {id}: {ex.Message} {ex.StackTrace}");
                throw new DataServiceException($"Ошибка при удалении дерева {id}: {ex.Message}");
            }
        }

        public TreeModel DeleteTreeNotSyncronized(Guid id)
        {
            throw new NotImplementedException();
        }

        public FormulaModel AddFormulaNotSyncronized(FormulaCreator item)
        {
            try
            {
                var id = Guid.NewGuid();
                var check = repositoryFormula.GetAll().FirstOrDefault(s => s.Name == item.Name);
                if (check != null)
                    throw new DataServiceException($"Формула с наименованием {item.Name} уже существует");

                if (item.IsDefault)
                {
                    var exDefault = repositoryFormula.GetAll().Where(s => s.IsDefault).FirstOrDefault();
                    exDefault.IsDefault = false;
                    repositoryFormula.Update(exDefault, true);
                }

                var result = repositoryFormula.Add(new Formula()
                {
                    IsDefault = item.IsDefault,
                    Text = item.Text,
                    Id = id,
                    Name = item.Name,
                    IsSync = false,
                    VersionDate = DateTimeOffset.Now
                }, true);
                if (result != null)
                    return mapper.Map<FormulaModel>(result);
                throw new DataServiceException($"Ошибка при добавлении формулы {item.Name}");
            }
            catch (DatabaseException)
            {
                throw;
            }
            catch (DataServiceException)
            {
                throw;
            }
            catch (Exception ex)
            {
                logger.LogError($"Ошибка при добавлении формулы {item.Name}: {ex.Message} {ex.StackTrace}");
                throw new DataServiceException($"Ошибка при добавлении формулы {item.Name}: {ex.Message}");
            }
        }

        public FormulaModel AddFormulaSyncronized(FormulaModel item, DateTimeOffset versionDate)
        {
            try
            {                
                var check = repositoryFormula.GetAll().FirstOrDefault(s => s.Name == item.Name);
                if (check != null)
                    throw new DataServiceException($"Формула с наименованием {item.Name} уже существует");

                if (item.IsDefault)
                {
                    var exDefault = repositoryFormula.GetAll().Where(s => s.IsDefault).FirstOrDefault();
                    exDefault.IsDefault = false;
                    repositoryFormula.Update(exDefault, true);
                }

                var result = repositoryFormula.Add(new Formula()
                {
                    IsDefault = item.IsDefault,
                    Text = item.Text,
                    Id = item.Id,
                    Name = item.Name,
                    IsSync = true,
                    VersionDate = versionDate
                }, true);
                if (result != null)
                    return mapper.Map<FormulaModel>(result);
                throw new DataServiceException($"Ошибка при добавлении формулы {item.Name}");
            }
            catch (DatabaseException)
            {
                throw;
            }
            catch (DataServiceException)
            {
                throw;
            }
            catch (Exception ex)
            {
                logger.LogError($"Ошибка при добавлении формулы {item.Name}: {ex.Message} {ex.StackTrace}");
                throw new DataServiceException($"Ошибка при добавлении формулы {item.Name}: {ex.Message}");
            }
        }

        public FormulaModel GetDefaultFormula()
        {
            try
            {
                var item = repositoryFormula.GetAll().Where(s=>s.IsDefault).FirstOrDefault();
                if (item != null)
                    return mapper.Map<FormulaModel>(item);
                throw new DataServiceException("Формула по умолчанию не найдена");
            }
            catch (DatabaseException)
            {
                throw;
            }
            catch (DataServiceException)
            {
                throw;
            }
            catch (Exception ex)
            {
                logger.LogError($"Ошибка получения формул: {ex.Message} {ex.StackTrace}");
                throw new DataServiceException($"Ошибка при получении формулы: {ex.Message}");
            }
        }

        public TreeItemModel AddTreeItemNotSyncronized(TreeItemCreator item)
        {
            try
            {
                var id = Guid.NewGuid();
                var check = repositoryTreeItem.GetAll().FirstOrDefault(s => s.Name == item.Name && s.TreeId == item.TreeId && s.ParentId == item.ParentId);
                if (check != null)
                    throw new DataServiceException($"Элемент с наименованием {item.Name} уже существует");
                            
                var result = repositoryTreeItem.Add(new TreeItem()
                {                   
                    Id = id,
                    Name = item.Name,
                    IsSync = false,
                    VersionDate = DateTimeOffset.Now,
                    AddFields = item.AddFields,
                    Description = item.Description,
                    IsDeleted = false,
                    IsLeaf = true,
                    ParentId = item.ParentId,
                    SelectCount = 0,
                    TreeId = item.TreeId,
                    Weight = item.Weight
                }, true);
                if (result != null)
                {
                    if (item.ParentId != null)
                    {
                        var parent = repositoryTreeItem.Get(item.ParentId.Value);
                        parent.IsLeaf = false;
                        repositoryTreeItem.Update(parent, true);
                    }
                    return mapper.Map<TreeItemModel>(result);
                }
                throw new DataServiceException($"Ошибка при добавлении элемента {item.Name}");
            }
            catch (DatabaseException)
            {
                throw;
            }
            catch (DataServiceException)
            {
                throw;
            }
            catch (Exception ex)
            {
                logger.LogError($"Ошибка при добавлении элемента {item.Name}: {ex.Message} {ex.StackTrace}");
                throw new DataServiceException($"Ошибка при добавлении элемента {item.Name}: {ex.Message}");
            }
        }

        public TreeItemModel AddTreeItemSyncronized(TreeItemModel r, DateTimeOffset d)
        {
            throw new NotImplementedException();
        }

        public FormulaModel UpdateFormulaSyncronized(FormulaModel res, DateTimeOffset now)
        {
            throw new NotImplementedException();
        }

        public TreeItemModel DeleteTreeItemNotSyncronized(Guid t)
        {
            throw new NotImplementedException();
        }

        public TreeItemModel DeleteTreeItemSyncronized(Guid id, DateTimeOffset d)
        {
            throw new NotImplementedException();
        }

        public FormulaModel UpdateFormulaNotSyncronized(FormulaUpdater t)
        {
            throw new NotImplementedException();
        }
    }
}