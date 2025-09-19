var builder = DistributedApplication.CreateBuilder(args);

builder.AddAzureFunctionsProject<Projects.Middleware_FX>("middleware-fx");

builder.Build().Run();
