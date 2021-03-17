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
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: Node/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: Node/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Node/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
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