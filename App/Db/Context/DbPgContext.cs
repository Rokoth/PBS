using ProjectBranchSelector.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ProjectBranchSelector.Db.Model;
using ProjectBranchSelector.Db.Model.Common;
using System;
using System.Linq;
using System.Reflection;

namespace ProjectBranchSelector.Db.Context
{
    public class DbPgContext : DbContext
    {        
        public DbSet<Tree> Trees { get; set; }
        public DbSet<TreeItem> TreeItems { get; set; }
        public DbSet<Formula> Formulas { get; set; }
        public DbSet<Settings> Settings { get; set; }

        public DbPgContext(DbContextOptions<DbPgContext> options) : base(options)
        {            
            
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.HasPostgresExtension("uuid-ossp");
            //Database.EnsureCreated();

            modelBuilder.ApplyConfiguration(new EntityConfiguration<Settings>());

            foreach (var type in Assembly.GetAssembly(typeof(Entity)).GetTypes())
            {
                if (typeof(Entity).IsAssignableFrom(type) && !type.IsAbstract)
                {
                    var configType = typeof(EntityConfiguration<>).MakeGenericType(type);
                    var config = Activator.CreateInstance(configType);
                    //typeof(ModelBuilder).GetMethod("ApplyConfiguration")
                    //    .MakeGenericMethod(type)
                    //    .Invoke(modelBuilder, new object[] { config });

                    GetType().GetMethod(nameof(ApplyConf), BindingFlags.NonPublic | BindingFlags.Instance)
                        .MakeGenericMethod(type).Invoke(this, new object[] { modelBuilder, config });


                }
            }
        }

        private void ApplyConf<T>(ModelBuilder modelBuilder, EntityConfiguration<T> config) where T: Entity
        {
            modelBuilder.ApplyConfiguration(config);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
           
        }
    }
}
