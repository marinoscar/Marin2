
var builder = DistributedApplication.CreateBuilder(args);

var servApp = builder.AddProject<Projects.Luval_Marin2_Services>("Marin2-Services");

var uiApp = builder.AddProject<Projects.Luval_Marin2_UI>("Marin2-UI")
    .WithReference(servApp);



builder.Build().Run();
