
using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Microsoft.Extensions.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

// Add secret for the Parameters:AzureAppConnString to the user screts to set the value
var azureAppConfig = builder.AddParameter("AzureAppConnString", true);


var sql = builder.AddSqlServer("sql")
                 .WithLifetime(ContainerLifetime.Persistent);

var marin2Db = sql.AddDatabase("marin2");


// Configure the UI project
var uiApp = builder.AddProject<Projects.Luval_Marin2_UI>("marin2-ui")
    .WithExternalHttpEndpoints()
    .WaitFor(sql)
    .WithReference(marin2Db)
    .WithEnvironment("AzureAppConnString", azureAppConfig);


builder.Build().Run();
