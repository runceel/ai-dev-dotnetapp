var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.EventRegistration_Web>("web");

builder.Build().Run();
