﻿using Luval.AuthMate.Core.Interfaces;
using Luval.AuthMate.Infrastructure.Data;
using Luval.AuthMate.Infrastructure.Logging;
using Luval.AuthMate.Sqlite;
using Luval.DbConnectionMate;
using Luval.GenAIBotMate.Infrastructure.Data;
using Luval.GenAIBotMate.Infrastructure.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management;
using Microsoft.SqlServer.Management.Smo;
using System.Text;

namespace Luval.Marin2.UI
{
    /// <summary>
    /// Helper class to initialize and check the database.
    /// </summary>
    public class DbHelper
    {
        private readonly IConfiguration _config;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="DbHelper"/> class.
        /// </summary>
        /// <param name="configuration">The configuration settings.</param>
        /// <param name="serviceProvider">The service provider.</param>
        /// <exception cref="ArgumentNullException">Thrown when configuration or serviceProvider is null.</exception>
        public DbHelper(IConfiguration configuration, IServiceProvider serviceProvider)
        {
            _config = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _logger = serviceProvider.GetRequiredService<ILogger<DbHelper>>();
        }

        /// <summary>
        /// Initializes the database if the configuration allows it.
        /// </summary>
        public void InitializeDb()
        {
            try
            {
                var runCheck = _config["Application:Database:CheckIfExists"];

                if (!string.IsNullOrEmpty(runCheck) && !bool.Parse(runCheck)) return;

                using (var scope = _serviceProvider.CreateScope())
                {
                    Task.Run(async () =>
                    {
                        await RunDbChecks(scope);
                    }).GetAwaiter().GetResult();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while initializing the database.");
                throw;
            }
        }

        /// <summary>
        /// Runs the database checks asynchronously.
        /// </summary>
        /// <param name="serviceScope">The service scope.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">Thrown when serviceScope is null.</exception>
        private async Task RunDbChecks(IServiceScope serviceScope)
        {
            if (serviceScope == null) throw new ArgumentNullException(nameof(serviceScope));

            try
            {
                var contextHelper = new AuthMateContextHelper(
                    serviceScope.ServiceProvider.GetRequiredService<IAuthMateContext>(),
                    serviceScope.ServiceProvider.GetRequiredService<ILogger<AuthMateContextHelper>>());

                var genContextHelp = new GenAIBotContextHelper(
                    serviceScope.ServiceProvider.GetRequiredService<IChatDbContext>(),
                    serviceScope.ServiceProvider.GetRequiredService<ILogger<GenAIBotContextHelper>>());

                await contextHelper.InitializeDbAsync(_config["OAuthProviders:Google:OwnerEmail"] ?? "someone@gmail.com");
                await genContextHelp.InitializeAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while running database checks.");
                throw;
            }
        }

        /// <summary>
        /// Checks if the SQL Server database exists and creates it if it does not.
        /// </summary>
        /// <param name="connectionString">The connection string to the SQL Server.</param>
        public void SqlServerCheckIfDatabaseExists(string connectionString)
        {
            try
            {
                LogConnectionString(connectionString);
                _logger.LogInformation("Checking if the database exists");
                var builder = new SqlConnectionStringBuilder(connectionString);
                var dbName = builder.InitialCatalog;
                var queryToCheck = $"SELECT COUNT(*) FROM sys.databases WHERE name = '{dbName}'";
                builder.InitialCatalog = "master";
                var conn = new SqlConnection(builder.ConnectionString);

                _logger.LogInformation($"Checking if the database exists with query: {queryToCheck}");
                var result = conn.ExecuteScalarAsync<int>(queryToCheck).GetAwaiter().GetResult();
                //need to create a database
                if (result <= 0)
                {
                    _logger.LogInformation("Database does not exist, creating it");
                    RunDatabaseScript(dbName, builder.ConnectionString);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while checking or creating the database.");
                throw;
            }
        }

        private void LogConnectionString(string connectionString)
        {
            var builder = new SqlConnectionStringBuilder(connectionString);
            builder.Password = "********";
            _logger.LogInformation($"Connection-String: {builder.ConnectionString}");
        }

        private string GetCreateDatabaseScript(string dbName)
        {
            return $@"
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = '{dbName}')
BEGIN
    CREATE DATABASE {dbName};
END;
GO

USE {dbName};
GO
";
        }

        private string GetFullScript(string dbName)
        {
            var sb = new StringBuilder();
            sb.AppendLine(GetCreateDatabaseScript(dbName));
            using (var scope = _serviceProvider.CreateScope())
            {
                sb.AppendLine();
                sb.AppendLine(scope.ServiceProvider.GetRequiredService<IAuthMateContext>().Database.GenerateCreateScript());
                sb.AppendLine();
                sb.AppendLine(scope.ServiceProvider.GetRequiredService<IChatDbContext>().Database.GenerateCreateScript());
            }
            return sb.ToString();
        }

        /// <summary>
        /// Runs the provided SQL script to create and initialize the database.
        /// </summary>
        /// <param name="dbName">The name of the database to create.</param>
        /// <param name="masterConnString">The connection string to the master database.</param>
        protected virtual void RunDatabaseScript(string dbName, string masterConnString)
        {
            try
            {
                _logger.LogInformation($"Running script to create and initialize the database: {dbName}");

                using (SqlConnection connection = new SqlConnection(masterConnString))
                {
                    var script = GetFullScript(dbName);
                    var server = new Server(new ServerConnection(connection));
                    server.ConnectionContext.ExecuteNonQuery(script);
                }

                _logger.LogInformation($"Successfully ran script to create and initialize the database: {dbName}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while running the script to create and initialize the database: {dbName}");
                throw;
            }
        }
    }
}
