
using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

// Add secret for the Parameters:AzureAppConnString to the user screts to set the value
var azureAppConfig = builder.AddParameter("AzureAppConnString", true);

// Add postgres instance with pgAdmin
var postgres = builder.AddPostgres("db")
    .WithDataVolume()
    .WithPgAdmin();

var authMateDb = postgres.AddDatabase("marin2");


// Configure the UI project
var uiApp = builder.AddProject<Projects.Luval_Marin2_UI>("marin2-ui")
    .WithExternalHttpEndpoints()
    .WaitFor(postgres)
    .WithReference(authMateDb)
    .WithEnvironment("AzureAppConnString", azureAppConfig);


builder.Build().Run();
