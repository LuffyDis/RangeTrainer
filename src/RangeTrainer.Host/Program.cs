var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddHealthChecks();

var app = builder.Build();

app.MapHealthChecks("/health");
app.MapGet("/", () => "RangeTrainer Host");

app.Run();

// Exposed so integration tests can drive the app via WebApplicationFactory<Program>.
public partial class Program;
