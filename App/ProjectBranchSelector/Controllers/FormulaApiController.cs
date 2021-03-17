using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ProjectBranchSelector.Models;
using ProjectBranchSelector.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ProjectBranchSelector.Controllers
{
    [Produces("application/json")]
    [Route("/api/v1/formula")]
    public class FormulaApiController : CommonApiControllerBase<FormulaModel, FormulaHistoryModel>
    {        

        public FormulaApiController(IServiceProvider serviceProvider): base(serviceProvider)
        {
            _logger = serviceProvider.GetRequiredService<ILogger<FormulaApiController>>();            
        }

        [HttpGet]
        public async Task<IActionResult> GetFormulas(string name = null, int page = 0, int size = 10, string sort = null, bool? isDefault = null)
        {
            if (isDefault.HasValue && isDefault.Value)
            {
                return await GetEntities(async (s, t) => (1, new List<FormulaModel> { await dataService.GetDefaultFormula(t) }),
                    new FormulaFilter(name, page, size, sort, true), "GetFormulas");                
            }
            return await GetEntities((s, t) => dataService.GetFormulas(s, t),
                new FormulaFilter(name, page, size, sort, true), "GetFormulas");           
        }

        [HttpGet("{id}")]       
        public async Task<IActionResult> GetFormula(Guid id)
        {
            return await GetEntity(dataService.GetFormula, id, "GetFormula");            
        }

        [HttpPost]
        public async Task<IActionResult> AddFormula([FromBody] FormulaCreator item)
        {
            return await ModifyItem(dataService.AddFormula, item, "AddFormula");            
        }

        [HttpPut]
        public async Task<IActionResult> UpdateFormula([FromBody] FormulaUpdater item)
        {
            return await ModifyItem(dataService.UpdateFormula, item, "UpdateFormula");            
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFormula([FromRoute] Guid id)
        {
            return await ModifyItem(dataService.DeleteFormula, id, "DeleteFormula");            
        }

        [HttpGet("history")]        
        public async Task<IActionResult> GetFormulaHistory(
           Guid? id = null,
           string name = null,
           DateTimeOffset? from = null,
           DateTimeOffset? to = null,
           int page = 0,
           int size = 10,
           string sort = null)
        {
            return await GetHistory(dataService.GetFormulaHistory, 
                new FormulaHistoryFilter(id, from, to, name, page, size, sort), "GetFormulaHistory");            
        }

        [HttpGet("changes")]
        public async Task<IActionResult> GetFormulaChanges(long lastHId, int size)
        {
            return await GetChanges(dataService.GetFormulaChanges, lastHId, size, "GetFormulaChanges");           
        }
    }
}
