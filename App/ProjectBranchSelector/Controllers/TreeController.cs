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
    public class TreeController : CommonControllerBase
    {       
        public TreeController(IServiceProvider serviceProvider): base(serviceProvider)
        {
            _logger = serviceProvider.GetRequiredService<ILogger<TreeController>>();            
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> TreeFiltered(string name = null, int page = 0, int size = 10, string sort = null)
        {
            try
            {
                var result = await dataService.GetTrees(new TreeFilter(name, page, size, sort, true), cancellationToken);                
                var pages = (result.Item1 % size == 0) ? (result.Item1 / size) : (result.Item1 / size) + 1;
                if (Response != null) Response.Headers.Add("x-pages", pages.ToString());
                return PartialView(result.Item2);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Method GetTrees exception: {ex.Message} + ST: {ex.StackTrace}");
                return RedirectToAction("Error", $"Method GetTrees exception: {ex.Message} + ST: {ex.StackTrace}");
            }
        }

        public async Task<IActionResult> TreeCount(string name = null, int page = 0, int size = 10, string sort = null)
        {
            try
            {
                return Ok(await dataService.GetTreesCount(new TreeFilter(name, page, size, sort, true), cancellationToken));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Method GetTrees exception: {ex.Message} + ST: {ex.StackTrace}");
                return RedirectToAction("Error", $"Method GetTrees exception: {ex.Message} + ST: {ex.StackTrace}");
            }
        }

        [HttpGet]
        public async Task<JsonResult> CheckName(string name)
        {
            bool result = false;
            if (!string.IsNullOrEmpty(name))
            {
                var check = await dataService.GetTrees(new TreeFilter(name, 0, 10, null), cancellationToken);
                result = !check.Item2.Any();
            }
            return Json(result);
        }

        [HttpGet]
        public async Task<JsonResult> CheckFormulaId(Guid id)
        {
            var check = await dataService.GetFormula(id, cancellationToken);
            var result = check!=null;
            return Json(result);
        }

        public async Task<IActionResult> ListSelect(string name = null, int page = 0, int size = 10, string sort = null)
        {
            try
            {
                return PartialView(await dataService.GetTrees(new TreeFilter(name, page, size, sort, true), cancellationToken));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Method GetTrees exception: {ex.Message} + ST: {ex.StackTrace}");
                return RedirectToAction("Error", $"Method GetTrees exception: {ex.Message} + ST: {ex.StackTrace}");
            }
        }

        // GET: Project/Details/5
        public async Task<IActionResult> Details(Guid id)
        {
            try
            {
                var model = await dataService.GetTree(id, cancellationToken);
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Method GetTree exception: {ex.Message} + ST: {ex.StackTrace}");
                return RedirectToAction("Error", $"Method GetTree exception: {ex.Message} + ST: {ex.StackTrace}");
            }
        }

        // GET: Project/Create
        public async Task<ActionResult> Create()
        {
            var formula = await dataService.GetDefaultFormula(cancellationToken);
            TreeModel item = new TreeModel()
            { 
               FormulaId = formula.Id,
               Formula = formula.Name
            };
            return View(item);
        }

        // POST: Project/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TreeModel item)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    TreeModel model = await dataService.AddTree(new TreeCreator()
                    { 
                      Description = item.Description,
                      FormulaId = item.FormulaId,
                      Name = item.Name
                    }, cancellationToken);
                    return RedirectToAction(nameof(Details), new { id = model.Id });
                }
                else
                {
                    var formula = await dataService.GetDefaultFormula(cancellationToken);
                    return View(item);
                }
            }
            catch(Exception ex)
            {
                return RedirectToAction("Error", $"Method AddTree exception: {ex.Message} + ST: {ex.StackTrace}");
            }
        }
                
        // GET: Project/Edit/5
        public async Task<IActionResult> Edit(Guid id)
        {
            try
            {
                TreeModel model = await dataService.GetTree(id, cancellationToken);
                return View(model);
            }
            catch (Exception ex)
            {
                return RedirectToAction("Error", $"Method GetTree exception: {ex.Message} + ST: {ex.StackTrace}");
            }
        }

        // POST: Project/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(TreeUpdater item)
        {
            try
            {
                TreeModel model = await dataService.UpdateTree(item, cancellationToken);                
                return RedirectToAction("Details", new { id = item.Id});
            }
            catch (Exception ex)
            {
                return RedirectToAction("Error", $"Method GetTree exception: {ex.Message} + ST: {ex.StackTrace}");
            }
        }

        // GET: Project/Delete/5
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                TreeModel model = await dataService.GetTree(id, cancellationToken);
                return View(model);
            }
            catch (Exception ex)
            {
                return RedirectToAction("Error", $"Method GetTree exception: {ex.Message} + ST: {ex.StackTrace}");
            }            
        }

        // POST: Project/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id, TreeModel item)
        {
            try
            {
                TreeModel model = await dataService.DeleteTree(item.Id, cancellationToken);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return RedirectToAction("Error", $"Method GetTree exception: {ex.Message} + ST: {ex.StackTrace}");
            }
        }
    }
}