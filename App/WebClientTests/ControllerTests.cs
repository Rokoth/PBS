using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using ProjectBranchSelector.Controllers;
using System;
using System.Threading.Tasks;
using Xunit;

namespace WebClientTests
{
    public class ControllerTests:IClassFixture<CustomFixture>
    {
        private ServiceProvider _serviceProvider;
        public ControllerTests(CustomFixture fixture)
        {
            _serviceProvider = fixture.ServiceProvider;
        }

        [Fact]
        public void HomeControllerTest()
        {            
            HomeController controller = new HomeController(_serviceProvider);
            var result = controller.Index() as ViewResult;
            Assert.NotNull(result);
        }

        [Fact]
        public async Task TreeControllerGetTest()
        {
            await TreeControllerGetPrepareTest();
            TreeController controller = new TreeController(_serviceProvider);
            var result = controller.Index() as ViewResult;
            Assert.NotNull(result);
        }

        private async Task TreeControllerGetPrepareTest()
        { 
            
        }
    }
}
