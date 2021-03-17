using ProjectBranchSelector.Common;
using Db.Repository;
using Deploy;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using ProjectBranchSelector.Db.Context;
using ProjectBranchSelector.Db.Interface;
using ProjectBranchSelector.Db.Model;
using ProjectBranchSelector.Service;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using AutoMapper;
using FormulaCalc;

namespace ProjectBranchSelector.DataServiceUnitTests
{
    public class CustomFixture: IDisposable
    {
        public string ConnectionString { get; private set; }
        public string RootConnectionString { get; private set; }
        public string DatabaseName { get; private set; }
        public ServiceProvider ServiceProvider { get; private set; }

        public CustomFixture()
        {            

            Log.Logger = new LoggerConfiguration()
             .MinimumLevel.Verbose() 
             .WriteTo.File(Path.Combine(Directory.GetCurrentDirectory(), "test-log.txt")) 
             .CreateLogger();

            var serviceCollection = new ServiceCollection();
            ConfigurationBuilder builder = new ConfigurationBuilder();
            builder.AddJsonFile("appsettings.json");            
            var config = builder.Build();

            DatabaseName = $"branch_selector_test_{DateTimeOffset.Now:yyyyMMdd_hhmmss}";
            ConnectionString = Regex.Replace(config.GetConnectionString("MainConnection"), "Database=.*?;", $"Database={DatabaseName};");
            RootConnectionString = Regex.Replace(config.GetConnectionString("MainConnection"), "Database=.*?;", $"Database=postgres;");
            serviceCollection.Configure<CommonOptions>(config);            
            serviceCollection.AddLogging(configure => configure.AddSerilog());
            serviceCollection.AddScoped<IDataService, DataService>();
            serviceCollection.AddScoped<IDeployService, DeployService>();
            serviceCollection.AddScoped<ICalculator, CalculatorNCalc>();
            serviceCollection.AddDbContext<DbPgContext>(opt=>opt.UseNpgsql(ConnectionString));
            serviceCollection.AddScoped<IRepository<Tree>, Repository<Tree>>();
            serviceCollection.AddScoped<IRepository<TreeItem>, Repository<TreeItem>>();
            serviceCollection.AddScoped<IRepository<Formula>, Repository<Formula>>();
            serviceCollection.AddScoped<IRepositoryHistory<TreeHistory>, RepositoryHistory<TreeHistory>>();
            serviceCollection.AddScoped<IRepositoryHistory<TreeItemHistory>, RepositoryHistory<TreeItemHistory>>();
            serviceCollection.AddScoped<IRepositoryHistory<FormulaHistory>, RepositoryHistory<FormulaHistory>>();
            serviceCollection.ConfigureAutoMapper();
            ServiceProvider = serviceCollection.BuildServiceProvider();

            ServiceProvider.GetRequiredService<IOptions<CommonOptions>>().Value.ConnectionString = ConnectionString;            
            ServiceProvider.GetRequiredService<IDeployService>().Deploy().GetAwaiter().GetResult();

        }
        
        public void Dispose()
        {            
            try
            {
                using NpgsqlConnection _connPg = new NpgsqlConnection(RootConnectionString);
                _connPg.Open();
                string script1 = "SELECT pg_terminate_backend (pg_stat_activity.pid) " +
                    $"FROM pg_stat_activity WHERE pid<> pg_backend_pid() AND pg_stat_activity.datname = '{DatabaseName}'; ";
                var cmd1 = new NpgsqlCommand(script1, _connPg);
                cmd1.ExecuteNonQuery();

                string script2 = $"DROP DATABASE {DatabaseName};";
                var cmd2 = new NpgsqlCommand(script2, _connPg);
                cmd2.ExecuteNonQuery();
            }
            catch
            {

            }
        }
    }
}
