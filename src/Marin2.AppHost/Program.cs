
using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Microsoft.Extensions.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

// Add secret for the Parameters:AzureAppConnString to the user screts to set the value
var azureAppConfig = builder.AddParameter("AzureAppConnString", true);

// Configure Azure Monitor
var insights = builder.AddConnectionString(
    "AzureMonitor",
    "APPLICATIONINSIGHTS_CONNECTION_STRING");



var sql = builder.AddSqlServer("sql")
                 .WithDataVolume();

var marin2Db = sql.AddDatabase("marin2");


// Configure the UI project
var uiApp = builder.AddProject<Projects.Luval_Marin2_UI>("marin2-ui")
    .WithExternalHttpEndpoints()
    .WaitFor(sql)
    .WithReference(insights)
    .WithReference(marin2Db)
    .WithEnvironment("AzureAppConnString", azureAppConfig);


builder.Build().Run();
