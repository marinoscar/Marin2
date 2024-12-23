
var builder = DistributedApplication.CreateBuilder(args);


// Add postgres instance with pgAdmin
var postgres = builder.AddPostgres("db")
    .WithPgAdmin();

var authMateDb = postgres.AddDatabase("authmate");

var servApp = builder.AddProject<Projects.Luval_Marin2_Services>("marin2-services");

var uiApp = builder.AddProject<Projects.Luval_Marin2_UI>("marin2-ui")
    .WithExternalHttpEndpoints()
    .WithReference(servApp)
    .WithReference(authMateDb);


builder.Build().Run();
