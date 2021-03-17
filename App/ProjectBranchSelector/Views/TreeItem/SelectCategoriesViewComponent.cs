using Microsoft.AspNetCore.Mvc;
using ProjectBranchSelector.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectBranchSelector.Views.TreeItem
{
    public class SelectCategoriesViewComponent : ViewComponent
    {
        public SelectCategoriesViewComponent()
        {
            
        }

        public async Task<IViewComponentResult> InvokeAsync(List<TreeItemModel> list, Guid parent)
        {            
            return await Task.Run(()=> View(list.Where(s => s.ParentId == parent)));
        }        
    }
}
