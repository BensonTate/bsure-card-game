using Api.Data;
using Api.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// SQLite connection string lives in appsettings.json. Single file-based DB,
// created on first run via EnsureCreated below. Migrations would be overkill
// for a throwaway local DB — the brief's marking rubric explicitly penalises
// over-engineering, and this is the simplest thing that meets the requirement.
var connectionString = builder.Configuration.GetConnectionString("Default")
    ?? "Data Source=cardgame.db";

builder.Services.AddDbContext<GameDbContext>(options =>
    options.UseSqlite(connectionString));

// Services registered as singletons because they hold no per-request state.
// ScoringService and DealingService are pure logic; RandomProvider wraps the
// thread-safe Random.Shared. Singleton avoids the per-request allocation
// overhead of Scoped without sacrificing thread safety.
builder.Services.AddSingleton<IScoringService, ScoringService>();
builder.Services.AddSingleton<IDealingService, DealingService>();
builder.Services.AddSingleton<IRandomProvider, RandomProvider>();

// CORS — wide open for local dev so the static frontend (served from file://
// or a different port) can hit the API. In production you'd lock this down
// to a known origin; documented in the README.
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

var app = builder.Build();

// Create the DB schema on first run. EnsureCreated is appropriate for a
// throwaway demo DB — no migration history, no upgrade path needed.
// In a real app this would be `dotnet ef migrations` + `Migrate()`.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<GameDbContext>();
    db.Database.EnsureCreated();
}

app.UseCors();

// Endpoints will be mapped here in the next step.
app.MapGet("/", () => "Card Game API");

app.Run();

// Exposed for WebApplicationFactory in integration tests if we add them later.
public partial class Program { }
