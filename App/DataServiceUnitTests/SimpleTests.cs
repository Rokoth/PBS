using Microsoft.Extensions.DependencyInjection;
using ProjectBranchSelector.Db.Context;
using ProjectBranchSelector.Models;
using ProjectBranchSelector.Service;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace ProjectBranchSelector.DataServiceUnitTests
{
    public class SimpleTests : IClassFixture<CustomFixture>
    {
        private readonly ServiceProvider _serviceProvider;        

        public SimpleTests(CustomFixture fixture)
        {
            _serviceProvider = fixture.ServiceProvider;            
        }

        [Fact]
        public async Task GetTreesTest()
        {
            var context = _serviceProvider.GetRequiredService<DbPgContext>();
            
            var formula = (await TestHelper.AddFormulas(context, 1, "formula_{0}", "Min(SelectCount)", false)).FirstOrDefault();
            await TestHelper.AddTrees(context, formula.Id, 10, "tree_{0}_description", "tree_select_{0}_name");
            await TestHelper.AddTrees(context, formula.Id, 10, "tree_{0}_description", "tree_not_select_{0}_name");            
            
            var dataService = _serviceProvider.GetRequiredService<IDataService>();
            var actualTrees = await dataService.GetTrees(new TreeFilter("tree_select", 0, 5, "Name"), CancellationToken.None);

            Assert.Equal(5, actualTrees.Item2.Count());
            foreach (var tree in actualTrees.Item2)
            {
                Assert.Contains("tree_select", tree.Name);
            }
        }
    }
}
