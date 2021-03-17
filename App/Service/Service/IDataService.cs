using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ProjectBranchSelector.Models;

namespace ProjectBranchSelector.Service
{
    public interface IDataService
    {
        Task<(int, IEnumerable<TreeModel>)> GetTrees(TreeFilter filter, CancellationToken cancellationToken);
        Task<TreeModel> GetTree(Guid id, CancellationToken cancellationToken);
        Task<(int, IEnumerable<TreeItemModel>)> GetTreeItems(Guid id, CancellationToken cancellationToken);
        Task<FormulaModel> GetFormula(Guid id, CancellationToken cancellationToken);
        Task<TreeItemModel> GetTreeItem(Guid id, CancellationToken cancellationToken);
        Task<TreeModel> AddTree(TreeCreator item, CancellationToken cancellationToken);        
        Task<TreeModel> UpdateTree(TreeUpdater item, CancellationToken cancellationToken);
        Task<TreeModel> DeleteTree(Guid id, CancellationToken cancellationToken);
        Task<IEnumerable<TreeItemModel>> AddTreeItems(TreeItemsUpdater request, CancellationToken cancellationToken);
        Task<(int, IEnumerable<TreeHistoryModel>)> GetTreeHistory(TreeHistoryFilter treeHistoryFilter, CancellationToken cancellationToken);
        Task<(int, IEnumerable<TreeItemHistoryModel>)> GetTreeItemsHistory(TreeItemsHistoryFilter treeItemsHistoryFilter, CancellationToken cancellationToken);
        Task<(int, IEnumerable<FormulaModel>)> GetFormulas(FormulaFilter formulaFilter, CancellationToken cancellationToken);
        Task<FormulaModel> AddFormula(FormulaCreator item, CancellationToken cancellationToken);
        Task<SelectResponse> SelectItem(SelectRequest item, CancellationToken cancellationToken);
        Task<int> GetTreesCount(TreeFilter treeFilter, CancellationToken cancellationToken);
        Task<TreeItemModel> AddTreeItem(TreeItemCreator item, CancellationToken cancellationToken);
        Task<FormulaModel> UpdateFormula(FormulaUpdater item, CancellationToken cancellationToken);
        Task<FormulaModel> DeleteFormula(Guid id, CancellationToken cancellationToken);
        Task<(int, IEnumerable<FormulaHistoryModel>)> GetFormulaHistory(FormulaHistoryFilter formulaHistoryFilter, CancellationToken cancellationToken);
        Task<TreeItemModel> UpdateTreeItem(TreeItemUpdater item, CancellationToken cancellationToken);
        Task<IEnumerable<TreeHistoryModel>> GetTreeChanges(ChangesFilter treeChangesFilter, CancellationToken cancellationToken);
        Task<TreeItemModel> DeleteTreeItem(Guid id, CancellationToken cancellationToken);
        SelectResponse SelectItemCustom(SelectRequestCustom item, CancellationToken cancellationToken);
        Task<IEnumerable<TreeItemHistoryModel>> GetTreeItemsChanges(ChangesFilter changesFilter, CancellationToken cancellationToken);
        Task<IEnumerable<FormulaHistoryModel>> GetFormulaChanges(ChangesFilter changesFilter, CancellationToken cancellationToken);
        Task<FormulaModel> GetDefaultFormula(CancellationToken cancellationToken);
    }
}