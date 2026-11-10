var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => "GD My Stash");
app.MapGet("/api/health", () => Results.Ok(new { status = "ok", version = "0.0.1" }));

app.Run();
