using Deploy;
using Microsoft.Extensions.DependencyInjection;
using ProjectBranchSelector.Service;

namespace WebClientTests
{
    public class CustomFixture
    {
        public CustomFixture()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddLogging();
            serviceCollection.AddScoped<IDataService, DataService>();
            serviceCollection.AddScoped<IDeployService, DeployService>();
            ServiceProvider = serviceCollection.BuildServiceProvider();
        }

        public ServiceProvider ServiceProvider { get; private set; }
    }
}
