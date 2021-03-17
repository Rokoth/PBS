using AutoMapper;
using DesktopApp.Service;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProjectBarchSelector.DesktopApp.Service;
using ProjectBranchSelector.BSHttpClient;
using ProjectBranchSelector.Common;
using ProjectBranchSelector.DbClient;
using ProjectBranchSelector.DbClient.Context;
using ProjectBranchSelector.DbClient.Interface;
using ProjectBranchSelector.DbClient.Repository;
using ProjectBranchSelector.DesktopApp.Service;
using ProjectBranchSelector.Models;
using Serilog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace DesktopApp
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        private ServiceProvider serviceProvider;

        public App()
        {
            ServiceCollection services = new ServiceCollection();
            ConfigureServices(services);
            serviceProvider = services.BuildServiceProvider();
        }

        private void ConfigureServices(ServiceCollection services)
        {
            Log.Logger = new LoggerConfiguration()
                     .MinimumLevel.Debug()
                     .WriteTo.Console()
                     .WriteTo.File(@"logs\Log-.txt", rollingInterval: RollingInterval.Day, rollOnFileSizeLimit: true, fileSizeLimitBytes: 100000)
                    .CreateLogger();

            var config = new Microsoft.Extensions.Configuration.ConfigurationBuilder()
                        .SetBasePath(Directory.GetCurrentDirectory())
                        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                        .AddEnvironmentVariables()
                        .AddDbConfiguration()
                        .Build();

            services.Configure<CommonOptions>(config);
            services.AddDbContext<DbSqLiteContext>(options =>
            {
                options.UseSqlite(config.GetConnectionString("MainConnection"));
            });
            services.AddLogging(loggingBuilder =>
                loggingBuilder.AddSerilog(Log.Logger, dispose: true)
            );            
            services.AddScoped<IRepository<Tree>, Repository<Tree>>();            
            services.AddScoped<IRepository<TreeItem>, Repository<TreeItem>>();           
            services.AddScoped<IRepository<Formula>, Repository<Formula>>();
            services.AddScoped<IRepository<SyncConflict>, Repository<SyncConflict>>();
            services.AddScoped<IBSHttpClient, BSHttpClient>();
            services.AddScoped<IDbService, DbService>();
            services.AddScoped<IDataService, DataService>();
            services.AddSingleton<ISyncService, SyncService>();
            services.AddScoped<IFileService, FileService>();
            services.AddScoped<IHttpClientSettings, HttpClientSettings>(s=>
            {
                return new HttpClientSettings(config.GetValue<string>("ServerAddress"));
            });
            services.AddSingleton<MainWindow>();
            services.AddTransient<ServerConnectWindow>();
            services.AddTransient<SyncConflictWindow>();
            services.AddTransient<AddTreeWindow>();
            services.AddTransient<ExportToFileWindow>();
            services.AddTransient<FormulaSelectWindow>();
            services.AddTransient<FormulaAddWindow>();
            services.AddTransient<AboutWindow>();
            services.AddTransient<ErrorReportWindow>();

            services.ConfigureAutoMapper();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);            
            var mainWindow = serviceProvider.GetService<MainWindow>();
            mainWindow.Show();
        }
    }

    public class HttpClientSettings : IHttpClientSettings
    {
        public HttpClientSettings(string server)
        {
            Server = server;
        }

        public Dictionary<Type, string> Apis => new Dictionary<Type, string>()
        {
            { typeof(TreeModel), "api/v1/tree" },
            { typeof(TreeItemModel), "api/v1/tree_item" },
            { typeof(FormulaModel), "api/v1/formula" },
            { typeof(TreeHistoryModel), "api/v1/tree/changes" },
            { typeof(TreeItemHistoryModel), "api/v1/tree/items/changes" },
            { typeof(FormulaHistoryModel), "api/v1/formula/changes" },
        };
        public string Server { get; private set; }
    }

    public static class CustomExtensionMethods
    {
        public static IServiceCollection ConfigureAutoMapper(this IServiceCollection services)
        {
            var mappingConfig = new MapperConfiguration(mc => mc.AddProfile(new MappingProfile()));

            var mapper = mappingConfig.CreateMapper();
            services.AddSingleton(mapper);
            return services;
        }
    }

    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<TreeCreator, Tree>()
                .ForMember(s => s.Id, s => s.MapFrom(c => Guid.NewGuid()))
                .ForMember(s => s.VersionDate, s => s.MapFrom(c => DateTimeOffset.Now));

            CreateMap<TreeUpdater, Tree>()
                .ForMember(s => s.Id, s => s.MapFrom(c => Guid.NewGuid()))
                .ForMember(s => s.VersionDate, s => s.MapFrom(c => DateTimeOffset.Now));

            CreateMap<Tree, TreeModel>()
                .ForMember(s => s.Formula, s => s.MapFrom(c => c.Formula.Name));

            CreateMap<TreeItem, TreeItemModel>();

            CreateMap<FormulaCreator, Formula>();

            CreateMap<FormulaUpdater, Formula>();

            CreateMap<Formula, FormulaModel>();
        }
    }

    public static class ConfigurationExtensions
    {
        public static IConfigurationBuilder AddDbConfiguration(this IConfigurationBuilder builder)
        {
            var configuration = builder.Build();
            var connectionString = configuration.GetConnectionString("MainConnection");
            builder.AddConfigDbProvider(options => options.UseSqlite(connectionString));
            return builder;
        }

        public static IConfigurationBuilder AddConfigDbProvider(
            this IConfigurationBuilder configuration, Action<DbContextOptionsBuilder> setup)
        {
            configuration.Add(new ConfigDbSource(setup));
            return configuration;
        }
    }

    public class ConfigDbSource : IConfigurationSource
    {
        private readonly Action<DbContextOptionsBuilder> _optionsAction;

        public ConfigDbSource(Action<DbContextOptionsBuilder> optionsAction)
        {
            _optionsAction = optionsAction;
        }

        public Microsoft.Extensions.Configuration.IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            return new ConfigDbProvider(_optionsAction);
        }
    }

    public class ConfigDbProvider : ConfigurationProvider
    {
        private readonly Action<DbContextOptionsBuilder> _options;

        public ConfigDbProvider(Action<DbContextOptionsBuilder> options)
        {
            _options = options;
        }

        public override void Load()
        {
            var builder = new DbContextOptionsBuilder<DbSqLiteContext>();
            _options(builder);

            using (var context = new DbSqLiteContext(builder.Options))
            {
                var items = context.Settings
                    .AsNoTracking()
                    .ToList();

                foreach (var item in items)
                {
                    Data.Add(item.ParamName, item.ParamValue);
                }
            }
        }
    }

}
