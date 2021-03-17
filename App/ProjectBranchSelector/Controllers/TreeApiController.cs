using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ProjectBranchSelector.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectBranchSelector.Controllers
{
    [Produces("application/json")]
    [Route("/api/v1/tree")]
    public class TreeApiController : CommonApiControllerBase<TreeModel, TreeHistoryModel>
    {
        public TreeApiController(IServiceProvider serviceProvider): base(serviceProvider)
        {
            _logger = serviceProvider.GetRequiredService<ILogger<TreeController>>();            
        }

        [HttpGet]       
        public async Task<IActionResult> GetTrees(string name = null, int page = 0, int size = 10, string sort = null)
        {
            return await GetEntities((s,t)=> dataService.GetTrees(s,t), 
                new TreeFilter(name, page, size, sort, true), "GetTrees");            
        }               

        [HttpGet("history")]
        
        public async Task<IActionResult> GetTreeHistory(Guid? id = null, string name = null, DateTimeOffset? from = null, 
            DateTimeOffset? to = null,  int page = 0, int size = 10, string sort = null)
        {
            return await GetHistory((s, t) => dataService.GetTreeHistory(s, t),
                new TreeHistoryFilter(id, from, to, name, page, size, sort), "GetTreeHistory");            
        }

        [HttpGet("changes")]
        public async Task<IActionResult> GetTreeChanges(long lastHId, int size)
        {
            return await GetChanges(dataService.GetTreeChanges, lastHId, size, "GetTreeChanges");            
        }

        [HttpGet("{id}")]       
        public async Task<IActionResult> GetTree(Guid id)
        {
            return await GetEntity(dataService.GetTree, id, "GetTree");            
        }

        [HttpGet("{id}/items")]       
        public async Task<IActionResult> GetTreeItems(Guid id)
        {
            return await GetEntitiesInternalCommon((ct) => dataService.GetTreeItems(id, ct), "GetTreeItems");
        }

        [HttpGet("{id}/items/human")]
        public async Task<IActionResult> GetTreeItemsHuman(Guid id)
        {
            return await GetEntitiesInternalCommon<TreeItemModelHuman>(async (ct) => {
                List<TreeItemModelHuman> response = new List<TreeItemModelHuman>();
                var result = await dataService.GetTreeItems(id, cancellationToken);
                response.AddRange(result.Item2.Where(s => s.ParentId == null).Select(s => new TreeItemModelHuman()
                {
                    AddFields = s.AddFields,
                    Childs = GetChilds(s.Id, result.Item2),
                    Description = s.Description,
                    Id = s.Id,
                    Name = s.Name,
                    SelectCount = s.SelectCount,
                    Weight = s.Weight
                }));
                return (result.Item1, response);
            }, "GetTreeItemsHuman");            
        }

        private List<TreeItemModelHuman> GetChilds(Guid parentId, IEnumerable<TreeItemModel> items)
        {
            return items.Where(s => s.ParentId == parentId).Select(s => new TreeItemModelHuman()
            {
                AddFields = s.AddFields,
                Childs = GetChilds(s.Id, items),
                Description = s.Description,
                Id = s.Id,
                Name = s.Name,
                SelectCount = s.SelectCount,
                Weight = s.Weight
            }).ToList();
        }

        [HttpPost("items")]
        public async Task<IActionResult> AddTreeItem([FromBody] TreeItemCreator item)
        {
            return await ModifyItemInternal(dataService.AddTreeItem, item, "AddTreeItem");            
        }

        [HttpPut("items")]
        public async Task<IActionResult> EditTreeItem([FromBody] TreeItemUpdater item)
        {
            return await ModifyItemInternal(dataService.UpdateTreeItem, item, "EditTreeItem");            
        }

        [HttpDelete("items/{id}")]
        public async Task<IActionResult> DeleteTreeItem([FromRoute] Guid id)
        {
            return await ModifyItemInternal(dataService.DeleteTreeItem, id, "DeleteTreeItem");            
        }

        [HttpGet("{id}/items/history")]       
        public async Task<IActionResult> GetTreeItemsHistory(
            Guid id,
            string name = null,
            DateTimeOffset? from = null,
            DateTimeOffset? to = null,
            int page = 0,
            int size = 10,
            string sort = null)
        {
            return await GetEntitiesInternal(dataService.GetTreeItemsHistory, 
                new TreeItemsHistoryFilter(id, from, to, name, page, size, sort, false), "GetTreeItemsHistory");
        }

        [HttpGet("items/changes")]

        public async Task<IActionResult> GetTreeItemsChanges(long lastHId, int size)
        {
            return await GetChangesInternal(dataService.GetTreeItemsChanges, lastHId, size, "GetTreeItemsChanges");            
        }

        [HttpPost]        
        public async Task<IActionResult> AddTree([FromBody] TreeCreator item)
        {
            return await ModifyItem(dataService.AddTree, item, "AddTree");            
        }

        [HttpPut]       
        public async Task<IActionResult> EditTree([FromBody] TreeUpdater item)
        {
            return await ModifyItem(dataService.UpdateTree, item, "EditTree");            
        }

        [HttpDelete("{id}")]       
        public async Task<IActionResult> DeleteTree([FromRoute] Guid id)
        {
            return await ModifyItem(dataService.DeleteTree, id, "DeleteTree");           
        }
    }
}