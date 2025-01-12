using Luval.AuthMate.Core.Interfaces;
using Luval.AuthMate.Infrastructure.Data;
using Luval.AuthMate.Infrastructure.Logging;
using Luval.AuthMate.Sqlite;
using Luval.GenAIBotMate.Infrastructure.Data;
using Luval.GenAIBotMate.Infrastructure.Interfaces;

namespace Luval.Marin2.UI
{
    /// <summary>
    /// Helper class to initialize and check the database.
    /// </summary>
    public class DbHelper
    {
        private readonly IConfiguration _config;
        private readonly IServiceProvider _serviceProvider;

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
        }

        /// <summary>
        /// Initializes the database if the configuration allows it.
        /// </summary>
        public void InitializeDb()
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

        /// <summary>
        /// Runs the database checks asynchronously.
        /// </summary>
        /// <param name="serviceScope">The service scope.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">Thrown when serviceScope is null.</exception>
        private async Task RunDbChecks(IServiceScope serviceScope)
        {
            if (serviceScope == null) throw new ArgumentNullException(nameof(serviceScope));

            var contextHelper = new AuthMateContextHelper(
                serviceScope.ServiceProvider.GetRequiredService<IAuthMateContext>(),
                serviceScope.ServiceProvider.GetRequiredService<ILogger<AuthMateContextHelper>>());

            var genContextHelp = new GenAIBotContextHelper(
                serviceScope.ServiceProvider.GetRequiredService<IChatDbContext>(),
                serviceScope.ServiceProvider.GetRequiredService<ILogger<GenAIBotContextHelper>>());

            await contextHelper.InitializeDbAsync(_config["OAuthProviders:Google:OwnerEmail"] ?? "someone@gmail.com");
            await genContextHelp.InitializeAsync();
        }
    }
}
