using ProjectBranchSelector.Models;
using System;
using System.Collections.Generic;

namespace ProjectBranchSelector.DesktopApp.Service
{
    public interface IDbService
    {
        (int, IEnumerable<TreeModel>) GetTrees(string name, int? page, int? size, string sort);
        (int, IEnumerable<TreeItemModel>) GetTreeItems(Guid id);
        FormulaModel GetFormula(Guid id);
        (int, IEnumerable<FormulaModel>) GetFormulas(string name, int? page, int? size, string sort);
        TreeModel GetTree(Guid id);
        TreeItemModel GetTreeItem(Guid id);
        void SaveSettings(string key, string value);        
        TreeModel AddTreeSyncronized(TreeModel result, DateTimeOffset versionDate);
        TreeModel AddTreeNotSyncronized(TreeCreator tree);
        TreeModel UpdateTreeSyncronized(TreeModel result, DateTimeOffset versionDate);
        TreeModel UpdateTreeNotSyncronized(TreeUpdater tree);
        string GetSettings(string key);
        TreeItemModel UpdateTreeItemSyncronized(TreeItemModel result, DateTimeOffset versionDate);
        TreeItemModel UpdateTreeItemNotSyncronized(TreeItemUpdater item);
        TreeModel DeleteTreeSyncronized(Guid id, DateTimeOffset now);
        TreeModel DeleteTreeNotSyncronized(Guid id);
        FormulaModel GetDefaultFormula();
        TreeItemModel AddTreeItemNotSyncronized(TreeItemCreator t);
        TreeItemModel AddTreeItemSyncronized(TreeItemModel r, DateTimeOffset d);
        FormulaModel AddFormulaSyncronized(FormulaModel item, DateTimeOffset versionDate);
        FormulaModel UpdateFormulaSyncronized(FormulaModel res, DateTimeOffset now);
        FormulaModel AddFormulaNotSyncronized(FormulaCreator t);
        TreeItemModel DeleteTreeItemNotSyncronized(Guid t);
        TreeItemModel DeleteTreeItemSyncronized(Guid id, DateTimeOffset d);
        FormulaModel UpdateFormulaNotSyncronized(FormulaUpdater t);
    }
}