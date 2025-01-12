
using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

// Add secret for the Parameters:authmate-bearingtokenkey to the user screts to set the value
var authMateKey = builder.AddParameter("authmate-bearingtokenkey", true);
var azureStorageConnString = builder.AddParameter("azure-storage-connstring", true);

// Add postgres instance with pgAdmin
var postgres = builder.AddPostgres("db")
    .WithDataVolume()
    .WithPgAdmin();

var authMateDb = postgres.AddDatabase("marin2");

// Configure the services project
var servApp = builder.AddProject<Projects.Luval_Marin2_Services>("marin2-services")
    .WithEnvironment("authmate-bearingtokenkey", authMateKey);


// Configure the UI project
var uiApp = builder.AddProject<Projects.Luval_Marin2_UI>("marin2-ui")
    .WithExternalHttpEndpoints()
    .WaitFor(postgres)
    .WithReference(servApp)
    .WithReference(authMateDb)
    .WithEnvironment("authmate-bearingtokenkey", authMateKey)
    .WithEnvironment("azure-storage-connstring", azureStorageConnString);


builder.Build().Run();
