using AutoMapper;
using ProjectBranchSelector.Common;
using Db.Repository;
using Deploy;
using FormulaCalc;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using ProjectBranchSelector.Db.Context;
using ProjectBranchSelector.Db.Interface;
using ProjectBranchSelector.Db.Model;
using ProjectBranchSelector.Service;

namespace ProjectBranchSelector
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CommonOptions>(Configuration);
            services.AddControllersWithViews();
            services.AddDbContextPool<DbPgContext>((opt) =>
            {
                opt.EnableSensitiveDataLogging();
                var connectionString = Configuration.GetConnectionString("MainConnection");
                opt.UseNpgsql(connectionString);
            });
            services.AddScoped<IRepository<Tree>, Repository<Tree>>();
            services.AddScoped<IRepository<TreeItem>, Repository<TreeItem>>();
            services.AddScoped<IRepository<Formula>, Repository<Formula>>();
            services.AddScoped<IRepositoryHistory<TreeHistory>, RepositoryHistory<TreeHistory>>();
            services.AddScoped<IRepositoryHistory<TreeItemHistory>, RepositoryHistory<TreeItemHistory>>();
            services.AddScoped<IRepositoryHistory<FormulaHistory>, RepositoryHistory<FormulaHistory>>();
            services.AddScoped<IDataService, DataService>();
            services.AddScoped<IDeployService, DeployService>();
            services.AddScoped<ICalculator, CalculatorNCalc>();
            services.ConfigureAutoMapper();
            services.AddSwaggerGen();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthorization();
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
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
 }
