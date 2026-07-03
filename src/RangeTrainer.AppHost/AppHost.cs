var builder = DistributedApplication.CreateBuilder(args);

var host = builder.AddProject<Projects.RangeTrainer_Host>("host");

builder.AddProject<Projects.RangeTrainer_Client>("client")
    .WithReference(host);

builder.Build().Run();
