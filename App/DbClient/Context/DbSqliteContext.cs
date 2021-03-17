using Microsoft.EntityFrameworkCore;

namespace ProjectBranchSelector.DbClient.Context
{
    public class DbSqLiteContext : DbContext
    {        
        public DbSet<Tree> Trees { get; set; }
        public DbSet<TreeItem> TreeItems { get; set; }
        public DbSet<Formula> Formulas { get; set; }
        public DbSet<Settings> Settings { get; set; }

        public DbSqLiteContext(DbContextOptions<DbSqLiteContext> options) : base(options){}

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new TreeEntityConfiguration());
            modelBuilder.ApplyConfiguration(new TreeItemEntityConfiguration());
            modelBuilder.ApplyConfiguration(new FormulaEntityConfiguration());
            modelBuilder.ApplyConfiguration(new SettingsEntityConfiguration());
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.EnableSensitiveDataLogging(true);
        }        
    }
}
