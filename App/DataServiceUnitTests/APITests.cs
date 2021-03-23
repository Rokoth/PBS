using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using ProjectBranchSelector.Controllers;
using ProjectBranchSelector.Db.Context;
using ProjectBranchSelector.Db.Model;
using ProjectBranchSelector.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace ProjectBranchSelector.DataServiceUnitTests
{
    public class APITests : IClassFixture<CustomFixture>
    {
        private readonly ServiceProvider _serviceProvider;

        public APITests(CustomFixture fixture)
        {
            _serviceProvider = fixture.ServiceProvider;
        }


        [Fact]
        public async Task GetTreesTest()
        {
            var context = _serviceProvider.GetRequiredService<DbPgContext>();

            var formulaId = Guid.NewGuid();

            context.Formulas.Add(new Formula()
            {
                Id = formulaId,
                IsDefault = true,
                IsDeleted = false,
                Name = $"formula_{formulaId}",
                Text = "Min(SelectCount)",
                VersionDate = DateTimeOffset.Now
            });

            await context.SaveChangesAsync();

            for (int i = 0; i < 10; i++)
            {
                var id = Guid.NewGuid();
                context.Trees.Add(new Tree()
                {
                    Description = $"tree_{id}_description",
                    FormulaId = formulaId,
                    Id = id,
                    IsDeleted = false,
                    Name = $"tree_select_{id}_name",
                    VersionDate = DateTimeOffset.Now
                });
            }

            for (int i = 0; i < 10; i++)
            {
                var id = Guid.NewGuid();
                context.Trees.Add(new Tree()
                {
                    Description = $"tree_{id}_description",
                    FormulaId = formulaId,
                    Id = id,
                    IsDeleted = false,
                    Name = $"tree_not_select_{id}_name",
                    VersionDate = DateTimeOffset.Now
                });
            }

            await context.SaveChangesAsync();

            TreeApiController controller = new TreeApiController(_serviceProvider);

            var result = (await controller.GetTrees("tree_select", 0, 5, "Name"));
            var okResult = result as OkObjectResult;
            Assert.NotNull(okResult);

            var trees = okResult.Value as IEnumerable<TreeModel>;
            Assert.Equal(5, trees.Count());
            foreach (var tree in trees)
            {
                Assert.Contains("tree_select", tree.Name);
            }            
        }

        [Fact]
        public async Task GetTreeTest()
        {
            var context = _serviceProvider.GetRequiredService<DbPgContext>();

            var formulaId = Guid.NewGuid();

            context.Formulas.Add(new Formula()
            {
                Id = formulaId,
                IsDefault = true,
                IsDeleted = false,
                Name = $"formula_{formulaId}",
                Text = "Min(SelectCount)",
                VersionDate = DateTimeOffset.Now
            });

            await context.SaveChangesAsync();

            var id = Guid.NewGuid();
            var controlTree = new Tree()
            {
                Description = $"tree_{id}_description",
                FormulaId = formulaId,
                Id = id,
                IsDeleted = false,
                Name = $"tree_select_{id}_name",
                VersionDate = DateTimeOffset.Now
            };
            context.Trees.Add(controlTree);
                        
            await context.SaveChangesAsync();

            TreeApiController controller = new TreeApiController(_serviceProvider);

            var result = await controller.GetTree(id);
            var okResult = result as OkObjectResult;
            Assert.NotNull(okResult);

            var actualTree = okResult.Value as TreeModel;
            Assert.Equal(controlTree.Name, actualTree.Name);
        }

        [Fact]
        public async Task AddTreeItemTest()
        {
            var context = _serviceProvider.GetRequiredService<DbPgContext>();

            var formulaId = Guid.NewGuid();

            context.Formulas.Add(new Formula()
            {
                Id = formulaId,
                IsDefault = true,
                IsDeleted = false,
                Name = $"formula_{formulaId}",
                Text = "Min(SelectCount)",
                VersionDate = DateTimeOffset.Now
            });

            await context.SaveChangesAsync();

            var id = Guid.NewGuid();
            var controlTree = new Tree()
            {
                Description = $"tree_{id}_description",
                FormulaId = formulaId,
                Id = id,
                IsDeleted = false,
                Name = $"tree_select_{id}_name",
                VersionDate = DateTimeOffset.Now
            };
            context.Trees.Add(controlTree);

            await context.SaveChangesAsync();

            TreeApiController controller = new TreeApiController(_serviceProvider);

            var controlTreeItem = new TreeItemCreator()
            {
                AddFields = "{}",
                Description = "TestTreeItem",
                Name = "TestTreeItem",
                ParentId = null,
                TreeId = id,
                Weight = 1
            };
            var result = await controller.AddTreeItem(controlTreeItem);
            var okResult = result as OkObjectResult;
            Assert.NotNull(okResult);

            var actualTreeItem = okResult.Value as TreeItemModel;
            Assert.Equal(controlTreeItem.Name, actualTreeItem.Name);

            var actualTreeItem2 = context.TreeItems.Where(s => s.TreeId == id).FirstOrDefault();
            Assert.NotNull(actualTreeItem2);
            Assert.Equal(controlTreeItem.Name, actualTreeItem2.Name);
        }

        [Fact]
        public async Task GetFormulasTest()
        {
            var context = _serviceProvider.GetRequiredService<DbPgContext>();
                        
            for (int i = 0; i < 10; i++)
            {
                var id = Guid.NewGuid();
                context.Formulas.Add(new Formula()
                {
                    Id = id,
                    IsDefault = true,
                    IsDeleted = false,
                    Name = $"formula_select_{id}",
                    Text = "Min(SelectCount)",
                    VersionDate = DateTimeOffset.Now
                });
            }

            for (int i = 0; i < 10; i++)
            {
                var id = Guid.NewGuid();
                context.Formulas.Add(new Formula()
                {
                    Id = id,
                    IsDefault = true,
                    IsDeleted = false,
                    Name = $"formula_not_select_{id}",
                    Text = "Min(SelectCount)",
                    VersionDate = DateTimeOffset.Now
                });
            }

            await context.SaveChangesAsync();

            FormulaApiController controller = new FormulaApiController(_serviceProvider);

            var result = (await controller.GetFormulas("formula_select", 0, 5, "Name"));
            var okResult = result as OkObjectResult;
            Assert.NotNull(okResult);

            var formulas = okResult.Value as IEnumerable<FormulaModel>;
            Assert.Equal(5, formulas.Count());
            foreach (var formula in formulas)
            {
                Assert.Contains("formula_select", formula.Name);
            }
        }

        [Fact]
        public async Task GetTreeHistoryTest()
        {
            var context = _serviceProvider.GetRequiredService<DbPgContext>();

            var formulaId = Guid.NewGuid();

            context.Formulas.Add(new Formula()
            {
                Id = formulaId,
                IsDefault = true,
                IsDeleted = false,
                Name = $"formula_{formulaId}",
                Text = "Min(SelectCount)",
                VersionDate = DateTimeOffset.Now
            });

            await context.SaveChangesAsync();

            var id1 = Guid.NewGuid();
            var tree1 = context.Trees.Add(new Tree()
            {
                Description = $"tree_{id1}_description",
                FormulaId = formulaId,
                Id = id1,
                IsDeleted = false,
                Name = $"tree_{id1}_name",
                VersionDate = DateTimeOffset.Now
            }).Entity;
            await context.SaveChangesAsync();

            var id2 = Guid.NewGuid();
            var tree2 = context.Trees.Add(new Tree()
            {
                Description = $"tree_{id2}_description",
                FormulaId = formulaId,
                Id = id2,
                IsDeleted = false,
                Name = $"tree_{id2}_name",
                VersionDate = DateTimeOffset.Now
            }).Entity;

            await context.SaveChangesAsync();

            TreeApiController controller = new TreeApiController(_serviceProvider);

            var result = await controller.GetTreeHistory(size:100);
            var okResult = result as OkObjectResult;
            Assert.NotNull(okResult);

            var items = (okResult.Value as IEnumerable<TreeHistoryModel>).OrderBy(s=>s.HId).ToList();
            Assert.Equal(2, items.Count());
                        
            Assert.Equal(tree1.Id, items[0].Id);
            Assert.Equal(tree2.Id, items[1].Id);

            tree1.Name = $"tree_{id1}_name2";
            context.Trees.Update(tree1);
            await context.SaveChangesAsync();
            tree2.IsDeleted = true;
            context.Trees.Update(tree2);
            await context.SaveChangesAsync();

            var result2 = await controller.GetTreeHistory(size: 100);
            var okResult2 = result2 as OkObjectResult;
            var items2 = (okResult2.Value as IEnumerable<TreeHistoryModel>).OrderBy(s => s.HId).ToList();
            Assert.Equal(4, items2.Count());

            Assert.Equal(tree1.Id, items2[2].Id);
            Assert.Equal($"tree_{id1}_name2", items2[2].Name);
            Assert.Equal(tree2.Id, items2[3].Id);
            Assert.True(items2[3].IsDeleted);
        }

        [Fact]
        public async Task GetTreeItemHistoryTest()
        {
            var context = _serviceProvider.GetRequiredService<DbPgContext>();

            var formulaId = Guid.NewGuid();

            context.Formulas.Add(new Formula()
            {
                Id = formulaId,
                IsDefault = true,
                IsDeleted = false,
                Name = $"formula_{formulaId}",
                Text = "Min(SelectCount)",
                VersionDate = DateTimeOffset.Now
            });

            await context.SaveChangesAsync();

            var treeId = Guid.NewGuid();
            var tree = context.Trees.Add(new Tree()
            {
                Description = $"tree_{treeId}_description",
                FormulaId = formulaId,
                Id = treeId,
                IsDeleted = false,
                Name = $"tree_{treeId}_name",
                VersionDate = DateTimeOffset.Now
            }).Entity;
            await context.SaveChangesAsync();

            var id1 = Guid.NewGuid();
            var treeItem1 = context.TreeItems.Add(new TreeItem()
            {
                Description = $"tree_item_{id1}_description",                
                Id = id1,
                IsDeleted = false,
                Name = $"tree_item_{id1}_name",
                AddFields = "{}",
                IsLeaf = true,
                ParentId = null,
                SelectCount = 0,
                TreeId = treeId,
                Weight = 100,
                VersionDate = DateTimeOffset.Now
            }).Entity;

            await context.SaveChangesAsync();

            var id2 = Guid.NewGuid();
            var treeItem2 = context.TreeItems.Add(new TreeItem()
            {
                Description = $"tree_item_{id2}_description",
                Id = id2,
                IsDeleted = false,
                Name = $"tree_item_{id2}_name",
                AddFields = "{}",
                IsLeaf = true,
                ParentId = null,
                SelectCount = 0,
                TreeId = treeId,
                Weight = 100,
                VersionDate = DateTimeOffset.Now
            }).Entity;

            await context.SaveChangesAsync();

            TreeApiController controller = new TreeApiController(_serviceProvider);

            var result = await controller.GetTreeItemsHistory(treeId, size: 100);
            var okResult = result as OkObjectResult;
            Assert.NotNull(okResult);

            var items = (okResult.Value as IEnumerable<TreeItemHistoryModel>).OrderBy(s => s.HId).ToList();
            Assert.Equal(2, items.Count());

            Assert.Equal(treeItem1.Id, items[0].Id);
            Assert.Equal(treeItem2.Id, items[1].Id);

            treeItem1.Name = $"tree_item_{id1}_name2";
            context.TreeItems.Update(treeItem1);
            await context.SaveChangesAsync();
            treeItem2.IsDeleted = true;
            context.TreeItems.Update(treeItem2);
            await context.SaveChangesAsync();

            var result2 = await controller.GetTreeItemsHistory(treeId, size: 100);
            var okResult2 = result2 as OkObjectResult;
            var items2 = (okResult2.Value as IEnumerable<TreeItemHistoryModel>).OrderBy(s => s.HId).ToList();
            Assert.Equal(4, items2.Count());

            Assert.Equal(treeItem1.Id, items2[2].Id);
            Assert.Equal($"tree_item_{id1}_name2", items2[2].Name);
            Assert.Equal(treeItem2.Id, items2[3].Id);
            Assert.True(items2[3].IsDeleted);
        }

        [Fact]
        public async Task AddTreeTest()
        {
            var context = _serviceProvider.GetRequiredService<DbPgContext>();

            var formulaId = Guid.NewGuid();

            context.Formulas.Add(new Formula()
            {
                Id = formulaId,
                IsDefault = true,
                IsDeleted = false,
                Name = $"formula_{formulaId}",
                Text = "Min(SelectCount)",
                VersionDate = DateTimeOffset.Now
            });

            await context.SaveChangesAsync();
                       
            TreeApiController controller = new TreeApiController(_serviceProvider);

            var controlTree = new TreeCreator()
            {                
                Description = "TestTree",
                Name = "TestTree",
                FormulaId = formulaId                
            };
            var result = await controller.AddTree(controlTree);
            var okResult = result as OkObjectResult;
            Assert.NotNull(okResult);

            var actualTree = okResult.Value as TreeModel;
            Assert.Equal(controlTree.Name, actualTree.Name);

            var actualTree2 = context.Trees.Where(s => s.Id == actualTree.Id).FirstOrDefault();
            Assert.NotNull(actualTree2);
            Assert.Equal(controlTree.Name, actualTree2.Name);
        }

        [Fact]
        public async Task GetTreeItemsTest()
        {
            var context = _serviceProvider.GetRequiredService<DbPgContext>();

            var formulaId = Guid.NewGuid();

            context.Formulas.Add(new Formula()
            {
                Id = formulaId,
                IsDefault = true,
                IsDeleted = false,
                Name = $"formula_{formulaId}",
                Text = "Min(SelectCount)",
                VersionDate = DateTimeOffset.Now
            });

            await context.SaveChangesAsync();

            var treeId = Guid.NewGuid();
            context.Trees.Add(new Tree()
            {
                Description = $"tree_{treeId}_description",
                FormulaId = formulaId,
                Id = treeId,
                IsDeleted = false,
                Name = $"tree_{treeId}_name",
                VersionDate = DateTimeOffset.Now
            });

            var treeId2 = Guid.NewGuid();
            context.Trees.Add(new Tree()
            {
                Description = $"tree_{treeId2}_description",
                FormulaId = formulaId,
                Id = treeId2,
                IsDeleted = false,
                Name = $"tree_{treeId2}_name",
                VersionDate = DateTimeOffset.Now
            });

            for (int i = 0; i < 10; i++)
            {
                var id = Guid.NewGuid();
                context.TreeItems.Add(new TreeItem()
                {
                    Description = $"tree_item_{id}_description",                    
                    Id = id,
                    IsDeleted = false,
                    Name = $"tree_item_{id}_name",
                    VersionDate = DateTimeOffset.Now,
                    AddFields = "{}",
                    IsLeaf = true,
                    ParentId = null,
                    SelectCount = 0,
                    TreeId = treeId,
                    Weight = 1
                });
            }

            for (int i = 0; i < 10; i++)
            {
                var id = Guid.NewGuid();
                context.TreeItems.Add(new TreeItem()
                {
                    Description = $"tree_item_{id}_description",
                    Id = id,
                    IsDeleted = false,
                    Name = $"tree_item_{id}_name",
                    VersionDate = DateTimeOffset.Now,
                    AddFields = "{}",
                    IsLeaf = true,
                    ParentId = null,
                    SelectCount = 0,
                    TreeId = treeId2,
                    Weight = 1
                });
            }

            await context.SaveChangesAsync();

            TreeApiController controller = new TreeApiController(_serviceProvider);

            var result = (await controller.GetTreeItems(treeId));
            var okResult = result as OkObjectResult;
            Assert.NotNull(okResult);

            var treeItems = okResult.Value as IEnumerable<TreeItemModel>;
            Assert.Equal(10, treeItems.Count());
            foreach (var treeItem in treeItems)
            {
                Assert.Equal(treeId, treeItem.TreeId);
            }
        }

        [Fact]
        public async Task GetFormulaTest()
        {
            var context = _serviceProvider.GetRequiredService<DbPgContext>();

            var formulaId = Guid.NewGuid();
            var control = new Formula()
            {
                Id = formulaId,
                IsDefault = true,
                IsDeleted = false,
                Name = $"formula_{formulaId}",
                Text = "Min(SelectCount)",
                VersionDate = DateTimeOffset.Now
            };
            context.Formulas.Add(control);

            await context.SaveChangesAsync();

            FormulaApiController controller = new FormulaApiController(_serviceProvider);

            var result = await controller.GetFormula(formulaId);
            var okResult = result as OkObjectResult;
            Assert.NotNull(okResult);

            var actual = okResult.Value as FormulaModel;
            Assert.Equal(control.Name, actual.Name);
        }

    }
}
