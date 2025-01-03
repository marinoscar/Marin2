using Luval.AuthMate.Core;
using Luval.AuthMate.Core.Interfaces;
using Luval.AuthMate.Core.Resolver;
using Luval.AuthMate.Infrastructure.Configuration;
using Luval.AuthMate.Infrastructure.Data;
using Luval.AuthMate.Postgres;
using Luval.Marin2.UI.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.FluentUI.AspNetCore.Components;
using Npgsql;

namespace Luval.Marin2.UI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var config = builder.Configuration;

            builder.AddServiceDefaults();

            // Add services to the container.
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();
            builder.Services.AddFluentUIComponents();

            // AuthMate: Add support for controllers
            builder.Services.AddControllers();
            builder.Services.AddHttpClient();
            builder.Services.AddHttpContextAccessor();

            //registers all of the services needed for AuthMate
            var connManager = new OAuthConnectionManager(config);
            var googleConfig = connManager.GetConfiguration("Google");

            var connStr = builder.Configuration.GetConnectionString("marin2");
            var key = Environment.GetEnvironmentVariable("authmate-bearingtokenkey");
            builder.Services.AddAuthMateServices(key ?? string.Empty,
                (s) =>
                {
                    var connStr = builder.Configuration.GetConnectionString("marin2");
                    return new PostgresAuthMateContext(connStr ?? string.Empty);
                });

            
            builder.Services.AddAuthMateGoogleAuth(new GoogleOAuthConfiguration()
            {
                // client id from your config file
                ClientId = googleConfig.ClientId,
                // the client secret from your config file
                ClientSecret = googleConfig.ClientSecret,
            });

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
            app.InitializeDb();

            app.Run();
        }
    }
}
