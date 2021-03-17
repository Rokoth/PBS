using Deploy;
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
    [Route("/api/v1/common")]
    public class CommonController : CommonControllerBase
    {        
        private readonly IDeployService deployService;

        public CommonController(IServiceProvider serviceProvider): base(serviceProvider)
        {
            _logger = serviceProvider.GetRequiredService<ILogger<TreeController>>();            
            deployService = serviceProvider.GetRequiredService<IDeployService>();           
        }

        [HttpGet("ping")]       
        public IActionResult Ping()
        {
            return Ok();
        }

        [HttpGet("deploy")]
        public async Task<IActionResult> Deploy()
        {
            try
            {
                await deployService.Deploy();
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка при раскатке базы данных: {ex.Message} {ex.StackTrace}");
                return InternalServerError($"Ошибка при раскатке базы данных: {ex.Message}");
            }
        }

        [HttpPost("select/common")]       
        public async Task<IActionResult> Select([FromBody]SelectRequest item)
        {
            try
            {
                return Ok(await dataService.SelectItem(item, cancellationToken));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Method Select exception: {ex.Message} + ST: {ex.StackTrace}");
                return InternalServerError($"Method Select exception: {ex.Message}");
            }
        }

        [HttpPost("select/custom")]
        public IActionResult SelectCustom([FromBody] SelectRequestCustom item)
        {
            try
            {
                return Ok(dataService.SelectItemCustom(item, cancellationToken));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Method SelectCustom exception: {ex.Message} + ST: {ex.StackTrace}");
                return InternalServerError($"Method SelectCustom exception: {ex.Message}");
            }
        }
    }
}