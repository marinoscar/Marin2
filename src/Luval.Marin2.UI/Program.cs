using Luval.AuthMate.Core;
using Luval.AuthMate.Core.Interfaces;
using Luval.AuthMate.Infrastructure.Data;
using Luval.AuthMate.Postgres;
using Luval.Marin2.UI.Components;
using Microsoft.FluentUI.AspNetCore.Components;

namespace Luval.Marin2.UI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.AddServiceDefaults();

            // Add services to the container.
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();
            builder.Services.AddFluentUIComponents();

            // AuthMate: Add support for controllers
            builder.Services.AddControllers();
            builder.Services.AddHttpClient();
            builder.Services.AddHttpContextAccessor();

            // Add AuthMate Services
            builder.Services.AddNpgsql<PostgresAuthMateContext>("marin2"); // Add the AuthMate database
            //registers all of the services needed for AuthMate
            builder.Services.AddAuthMateServices(Environment.GetEnvironmentVariable("authmate-bearingtokenkey") ?? string.Empty,
                (s) => {
                        var i = s.GetRequiredService<PostgresAuthMateContext>();
                        return i;
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
