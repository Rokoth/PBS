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
    public class FormulaController : CommonControllerBase
    {       
        public FormulaController(IServiceProvider serviceProvider): base(serviceProvider)
        {
            _logger = serviceProvider.GetRequiredService<ILogger<FormulaController>>();            
        }

        // GET: FormulaController
        public ActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Filtered(string name = null, int page = 0, int size = 10, string sort = null)
        {
            try
            {
                var result = await dataService.GetFormulas(new FormulaFilter(name, page, size, sort, true), cancellationToken);
                var pages = (result.Item1 % size == 0) ? (result.Item1 / size) : (result.Item1 / size) + 1;
                if (Response != null) Response.Headers.Add("x-pages", pages.ToString());
                return PartialView(result.Item2);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Method GetFormulas exception: {ex.Message} + ST: {ex.StackTrace}");
                return RedirectToAction("Error", $"Method GetFormulas exception: {ex.Message} + ST: {ex.StackTrace}");
            }
        }

        public ActionResult ListSelect()
        {
            return PartialView();
        }

        public async Task<IActionResult> ListSelectFiltered(string name = null, int page = 0, int size = 10, string sort = null)
        {
            try
            {
                var result = await dataService.GetFormulas(new FormulaFilter(name, page, size, sort, true), cancellationToken);
                Response.Headers.Add("x-pages", result.Item1.ToString());
                return PartialView(result.Item2);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Method GetFormulas exception: {ex.Message} + ST: {ex.StackTrace}");
                return RedirectToAction("Error", $"Method GetFormulas exception: {ex.Message} + ST: {ex.StackTrace}");
            }
        }

        // GET: FormulaController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: FormulaController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: FormulaController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: FormulaController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: FormulaController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: FormulaController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: FormulaController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
