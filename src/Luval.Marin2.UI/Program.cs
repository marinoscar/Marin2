using Luval.AuthMate.Core;
using Luval.AuthMate.Core.Interfaces;
using Luval.AuthMate.Core.Resolver;
using Luval.AuthMate.Infrastructure.Configuration;
using Luval.AuthMate.Infrastructure.Data;
using Luval.AuthMate.Postgres;
using Luval.AuthMate.SqlServer;
using Luval.GenAIBotMate.Infrastructure.Configuration;
using Luval.GenAIBotMate.Infrastructure.Data;
using Luval.Marin2.UI.Components;
using Luval.WorkMate.Infrastructure.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.FluentUI.AspNetCore.Components;
using Npgsql;
using System.Text;

namespace Luval.Marin2.UI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            //Gets the Azure App Configuration connection string from the configuration
            //All configuration values are on https://portal.azure.com/ App Configuration Marin2
            var azureAppConfigConnString = Environment.GetEnvironmentVariable("AzureAppConnString");

            //Add Azure App Configuration
            builder.Configuration.AddAzureAppConfiguration(cfg => {
                cfg.Connect(azureAppConfigConnString);
            });

            var config = builder.Configuration;

            builder.AddServiceDefaults();

            // Add services to the container.
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();
            builder.Services.AddFluentUIComponents();

            //Add logging services is a required dependency for AuthMate
            builder.Services.AddLogging();

            // AuthMate: Add support for controllers
            builder.Services.AddControllers();
            builder.Services.AddHttpClient();
            builder.Services.AddHttpContextAccessor();

            var connStr = config.GetConnectionString("marin2");

            //Add the AuthMate services
            builder.Services.AddAuthMateServices(
                //The key to use for the bearing token implementation
                config["AuthMate:BearingTokenKey"] ?? string.Empty,
                (s) =>
                {
                    //returns a local instance of Sqlite
                    
                    return new SqlServerAuthMateContext(connStr ?? string.Empty);
                });


            //Add the AuthMate Google OAuth provider
            builder.Services.AddAuthMateGoogleAuth(new GoogleOAuthConfiguration()
            {
                // client id from your config file
                ClientId = config["OAuthProviders:Google:ClientId"] ?? throw new ArgumentNullException("The Google client id is required"),
                // the client secret from your config file
                ClientSecret = config["OAuthProviders:Google:ClientSecret"] ?? throw new ArgumentNullException("The Google client secret is required"),
                // set the login path in the controller and pass the provider name
                LoginPath = "/api/auth",
            });

            //Add the Open AI capabilities 
            builder.Services.AddGenAIBotServicesWithSqlServer(
                config.GetValue<string>("OpenAIKey") ?? throw new ArgumentNullException("OpenAI Key is missing in the configuration"),
                connStr ?? throw new ArgumentNullException("Connection string is missing in the configuration"),
                config.GetValue<string>("Azure:Storage:ConnectionString") ?? throw new ArgumentNullException("Azure storage connection string missing in the configuration")
            );

            //Add the WorkMate services
            builder.Services.AddWorkMateServices();

            var app = builder.Build();

            app.MapDefaultEndpoints();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            /*** AuthMate: Additional configuration  ****/
            app.MapControllers();
            app.UseRouting();
            app.UseAuthorization();
            app.UseAuthentication();
            /*** AuthMate:                           ****/

            app.UseStaticFiles();
            app.UseAntiforgery();

            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();

            // AuthMate: Initialize the database
            var dbHelper = new DbHelper(config, app.Services);

            //When using sql server
            dbHelper.SqlServerCheckIfDatabaseExists(connStr);
            dbHelper.InitializeDb();

            app.Run();
        }
    }
}
