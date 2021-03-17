using Deploy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ProjectBranchSelector.Models;
using ProjectBranchSelector.Service;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace ProjectBranchSelector.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IDataService _dataService;
        private readonly IDeployService _deployService;

        public HomeController(IServiceProvider serviceProvider)
        {
            _logger = serviceProvider.GetRequiredService<ILogger<HomeController>>();
            _dataService = serviceProvider.GetRequiredService<IDataService>();
            _deployService = serviceProvider.GetRequiredService<IDeployService>(); 
        }

        public IActionResult Index(MessageModel mess = null)
        {
            return View(mess);
        }

        public async Task<IActionResult> Deploy()
        {
            try
            {
                await _deployService.Deploy();
                return RedirectToAction(nameof(Index), new { mess = new MessageModel { Message = "База данных установлена", IsError = false } });
            }
            catch (Exception)
            {
                return RedirectToAction(nameof(Index), new { mess = new MessageModel { Message = "Ошибка установки базы данных", IsError = true } });
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }


}
