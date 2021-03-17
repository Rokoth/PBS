using ProjectBranchSelector.Models;
using System;
using System.Collections.Generic;

namespace ProjectBarchSelector.DesktopApp.Service
{
    public interface IFileService
    {
        IEnumerable<TreeItemModel> GetTreeItemsFromFS(string path, Guid treeId);
    }
}