using ProjectBranchSelector.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProjectBranchSelector.DesktopApp.Service
{
    public interface IDataService
    {
        Task<FormulaModel> GetFormula(Guid id);
        Task<(int, IEnumerable<TreeItemModel>)> GetTreeItems(Guid id);
        Task<(int, IEnumerable<TreeModel>)> GetTrees(string name, int? page, int? size, string sort);
        Task<TreeModel> GetTree(Guid id);
        Task<(int, IEnumerable<FormulaModel>)> GetFormulas(string name, int? page, int? size, string sort);
        Task<TreeItemModel> GetTreeItem(Guid id);
        Task<TreeModel> AddTree(TreeCreator tree);
        Task<TreeModel> UpdateTree(TreeUpdater tree);
        Task<TreeModel> DeleteTree(Guid id);
        Task<TreeItemModel> AddTreeItem(TreeItemCreator item);
        Task<TreeItemModel> UpdateTreeItem(TreeItemUpdater item);
        Task<Guid?> GetDefaultFormula();
        Task<FormulaModel> AddFormula(FormulaCreator formulaCreator);
        Task<TreeItemModel> DeleteTreeItem(Guid id);
        Task<FormulaModel> UpdateFormula(FormulaUpdater formulaUpdater);
    }
}