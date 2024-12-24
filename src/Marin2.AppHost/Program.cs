
var builder = DistributedApplication.CreateBuilder(args);


// Add postgres instance with pgAdmin
var postgres = builder.AddPostgres("db")
    .WithPgAdmin();

var authMateDb = postgres.AddDatabase("marin2");

var servApp = builder.AddProject<Projects.Luval_Marin2_Services>("marin2-services");

// Add secret for the Parameters:authmate-bearingtokenkey to the user screts to set the value
var authMateKey = builder.AddParameter("authmate-bearingtokenkey", true);


var uiApp = builder.AddProject<Projects.Luval_Marin2_UI>("marin2-ui")
    .WithExternalHttpEndpoints()
    .WithReference(servApp)
    .WithReference(authMateDb)
    .WithEnvironment("authmate-bearingtokenkey", authMateKey);


builder.Build().Run();
