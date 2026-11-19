using HypertrophyApp.Application;
using HypertrophyApp.Application.Exercises;
using HypertrophyApp.Infrastructure;
using HypertrophyApp.Infrastructure.Persistence;
using HypertrophyApp.Infrastructure.Persistence.Seed;
using System.Threading.RateLimiting;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();

// There are no accounts, so throttle API calls per client IP to keep the public API from being hammered.
builder.Services.AddRateLimiter(o =>
{
    o.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    o.AddPolicy("api", ctx =>
        RateLimitPartition.GetFixedWindowLimiter(
            ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions { PermitLimit = 120, Window = TimeSpan.FromMinutes(1) }));
});

builder.Services.AddApplication(new ExerciseMediaOptions(
    builder.Configuration["ExerciseLibrary:ImageBaseUrl"]
    ?? throw new InvalidOperationException("ExerciseLibrary:ImageBaseUrl is not configured.")));
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment() || app.Configuration.GetValue<bool>("Database:MigrateOnStartup"))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<HypertrophyAppDbContext>();
    await db.Database.MigrateAsync();
    await ExerciseSeeder.SeedAsync(db, app.Logger);
}

app.UseExceptionHandler();
app.UseStatusCodePages();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts(); // TLS itself is terminated by the hosting proxy.
}

// Basic hardening headers for every response.
app.Use(async (ctx, next) =>
{
    ctx.Response.Headers["X-Content-Type-Options"] = "nosniff";
    ctx.Response.Headers["X-Frame-Options"] = "DENY";
    ctx.Response.Headers["Referrer-Policy"] = "no-referrer";
    await next();
});

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseWebAssemblyDebugging();
}

// The API also serves the Blazor WebAssembly client, so the whole app runs from a single origin.
// MapStaticAssets serves pre-compressed static files with caching headers.
app.UseRateLimiter();
app.MapStaticAssets();

app.MapControllers().RequireRateLimiting("api");
app.MapFallbackToFile("index.html");

app.Run();

public partial class Program; // Exposed for WebApplicationFactory in integration tests.
