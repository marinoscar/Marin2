var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.Luval_Marin2_UI>("luval-marin2-ui");

builder.AddProject<Projects.Luval_Marin2_Services>("luval-marin2-services");

builder.Build().Run();
