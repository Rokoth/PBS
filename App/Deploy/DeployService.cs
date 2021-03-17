using ProjectBranchSelector.Common;
using Deploy.Common;
using Deployer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Threading.Tasks;
using Npgsql;
using System.Text.RegularExpressions;

namespace Deploy
{
    public class DeployService : IDeployService
    {
        private readonly ILogger<DeployService> _logger;
        private readonly IOptions<CommonOptions> _options;

        public DeployService(IServiceProvider serviceProvider)
        {
            _logger = serviceProvider.GetRequiredService<ILogger<DeployService>>();
            _options = serviceProvider.GetRequiredService<IOptions<CommonOptions>>();
        }

        public async Task Deploy(int? num = null)
        {
            var deployLog = string.Empty;
            try
            {
                CheckDbForExists(_options.Value.ConnectionString);                

                DeploySettings deploySettings = new DeploySettings()
                {
                    BeginNum = num,
                    CheckSqlPath = Path.Combine(Directory.GetCurrentDirectory(), "Scripts", "Check"),
                    ConnectionString = _options.Value.ConnectionString,
                    DeploySqlPath = Path.Combine(Directory.GetCurrentDirectory(), "Scripts", "Deploy"),
                    UpdateSqlPath = Path.Combine(Directory.GetCurrentDirectory(), "Scripts", "Update")
                };
                var deployer = new Deployer.Deployer(deploySettings);
                deployer.OnError += (sender, message) =>
                {
                    deployLog += message + "\r\n";
                    _logger.LogError(message);
                };
                deployer.OnDebug += (sender, message) =>
                {
                    _logger.LogDebug(message);
                };
                deployer.OnMessage += (sender, message) =>
                {
                    _logger.LogInformation(message);
                };
                deployer.OnWarning += (sender, message) =>
                {
                    deployLog += message + "\r\n";
                    _logger.LogWarning(message);
                };

                

                if (!await deployer.Deploy())
                {
                    throw new DeployException($"DB was not deploy, log: {deployLog}");
                }
            }
            catch (DeployException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new DeployException($"" +
                    $"Error while Deploy DB.\r\n" +
                    $"Message: {ex.Message}\r\n" +
                    $"StackTrace: {ex.StackTrace}\r\n" +
                    $"DeployLog: {deployLog}");
            }
        }

        private void CheckDbForExists(string connectionString)
        {
            try
            {
                var dbName = Regex.Match(connectionString, "Database=(.*?);").Groups[1].Value;
                var rootConnectionString = Regex.Replace(connectionString, "Database=.*?;", $"Database=postgres;");
                using NpgsqlConnection _connPg = new NpgsqlConnection(rootConnectionString);
                _connPg.Open();
                string script1 = $"select exists(SELECT 1 FROM pg_database WHERE datname = '{dbName}');";
                var cmd1 = new NpgsqlCommand(script1, _connPg);
                if (!(bool)cmd1.ExecuteScalar())
                {
                    string script2 = $"create database {dbName};";
                    var cmd2 = new NpgsqlCommand(script2, _connPg);
                    cmd2.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw new DeployException($"Не удалось развернуть базу данных: " +
                    $"ошибка при проверке или создании базы: {ex.Message} {ex.StackTrace}");
            }
        }
    }
}
