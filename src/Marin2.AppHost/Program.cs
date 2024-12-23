
var builder = DistributedApplication.CreateBuilder(args);

var servApp = builder.AddProject<Projects.Luval_Marin2_Services>("marin2-services");

var uiApp = builder.AddProject<Projects.Luval_Marin2_UI>("marin2-ui")
    .WithReference(servApp);



builder.Build().Run();
