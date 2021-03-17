using ProjectBranchSelector.Db.Context;
using ProjectBranchSelector.Db.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProjectBranchSelector.DataServiceUnitTests
{
    public static class TestHelper
    {
        public static async Task<IEnumerable<Tree>> AddTrees(DbPgContext context, Guid formulaId,
                int count, string descriptionMask, string nameMask)
        {
            var result = new List<Tree>();
            for (int i = 0; i < count; i++)
            {
                var id = Guid.NewGuid();
                result.Add(context.Trees.Add(new Tree()
                {
                    Description = string.Format(descriptionMask, id, i),
                    FormulaId = formulaId,
                    Id = id,
                    IsDeleted = false,
                    Name = string.Format(nameMask, id, i),
                    VersionDate = DateTimeOffset.Now
                }).Entity);
            }
            await context.SaveChangesAsync();
            return result;
        }

        public static async Task<IEnumerable<Formula>> AddFormulas(DbPgContext context,
            int count, string nameMask, string text, bool isDefault)
        {
            var result = new List<Formula>();
            for (int i = 0; i < count; i++)
            {
                var id = Guid.NewGuid();
                result.Add(context.Formulas.Add(new Formula()
                {
                    Id = id,
                    IsDefault = isDefault,
                    IsDeleted = false,
                    Name = string.Format(nameMask, id, i),
                    Text = text,
                    VersionDate = DateTimeOffset.Now
                }).Entity);
            }
            await context.SaveChangesAsync();
            return result;
        }
    }
}
