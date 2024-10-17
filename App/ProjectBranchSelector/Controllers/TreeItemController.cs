using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ProjectBranchSelector.Models;
using ProjectBranchSelector.Service;

namespace ProjectBranchSelector.Controllers
{
    public class TreeItemController : Controller
    {
        private readonly ILogger<TreeController> _logger;
        private readonly IDataService dataService;
        private readonly CancellationToken cancellationToken;
        private readonly CancellationTokenSource tokenSource;

        public TreeItemController(IServiceProvider serviceProvider)
        {
            _logger = serviceProvider.GetRequiredService<ILogger<TreeController>>();
            dataService = serviceProvider.GetRequiredService<IDataService>();
            tokenSource = new CancellationTokenSource();
            cancellationToken = tokenSource.Token;
        }

        // GET: Node
        public async Task<IActionResult> Index(Guid treeId)
        {
            try
            {
                return View(await dataService.GetTreeItems(treeId, cancellationToken));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Method GetTreeItems exception: {ex.Message} + ST: {ex.StackTrace}");
                return RedirectToAction("Error", $"Method GetTreeItems exception: {ex.Message} + ST: {ex.StackTrace}");
            }
        }

        public async Task<IActionResult> ListSelect(Guid treeId)
        {
            try
            {
                return View(await dataService.GetTreeItems(treeId, cancellationToken));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Method GetTreeItems exception: {ex.Message} + ST: {ex.StackTrace}");
                return RedirectToAction("Error", $"Method GetTreeItems exception: {ex.Message} + ST: {ex.StackTrace}");
            }
        }

        // GET: Node/Details/5
        public async Task<IActionResult> Details(Guid id)
        {
            try
            {
                return View(await dataService.GetTreeItem(id, cancellationToken));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Method GetTreeItems exception: {ex.Message} + ST: {ex.StackTrace}");
                return RedirectToAction("Error", $"Method GetTreeItems exception: {ex.Message} + ST: {ex.StackTrace}");
            }            
        }

        // GET: Node/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Node/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(TreeItemCreator item)
        {
            try
            {
                TreeItemModel model = await dataService.AddTreeItem(item, cancellationToken);
                return RedirectToAction(nameof(Details), new { id = model.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Method AddTreeItem exception: {ex.Message} + ST: {ex.StackTrace}");
                return RedirectToAction("Error", $"Method AddTreeItem exception: {ex.Message} + ST: {ex.StackTrace}");
            }
        }

        // GET: Node/Edit/5
        public async Task<IActionResult> Edit(Guid id)
        {
            try
            {
                var model = await dataService.GetTreeItem(id, cancellationToken);
                return View(model);
            }
            catch (Exception ex)
            {
                return RedirectToAction("Error", $"Method GetTreeItem exception: {ex.Message} + ST: {ex.StackTrace}");
            }
        }

        // POST: Node/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(TreeItemUpdater item)
        {
            try
            {
                var model = await dataService.UpdateTreeItem(item, cancellationToken);
                return RedirectToAction("Details", new { id = item.Id });
            }
            catch (Exception ex)
            {
                return RedirectToAction("Error", $"Method UpdateTreeItem exception: {ex.Message} + ST: {ex.StackTrace}");
            }
        }

        // GET: Node/Delete/5
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var model = await dataService.GetTreeItem(id, cancellationToken);
                return View(model);
            }
            catch (Exception ex)
            {
                return RedirectToAction("Error", $"Method GetTreeItem exception: {ex.Message} + ST: {ex.StackTrace}");
            }
        }

        // POST: Node/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id, TreeItemModel item)
        {
            try
            {
                var model = await dataService.DeleteTreeItem(item.Id, cancellationToken);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return RedirectToAction("Error", $"Method DeleteTreeItem exception: {ex.Message} + ST: {ex.StackTrace}");
            }
        }

        [HttpGet]
        public async Task<JsonResult> CheckName(string name, Guid treeId, Guid? parentId)
        {
            var check = (await dataService.GetTreeItems(treeId, cancellationToken)).Item2
                .Where(s=>s.ParentId == parentId && s.Name == name);
            var result = !check.Any();
            return Json(result);
        }
    }
}