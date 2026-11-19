using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Testcontainers.PostgreSql;

namespace HypertrophyApp.IntegrationTests;

/// <summary>
/// Boots the real API against a throw-away PostgreSQL container, so the tests exercise the same
/// EF Core migrations, jsonb/text[] mappings and seed data as production.
/// </summary>
public sealed class ApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _db = new PostgreSqlBuilder("postgres:17-alpine").Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("ConnectionStrings:HypertrophyApp", _db.GetConnectionString());
        builder.UseSetting("Database:MigrateOnStartup", "true");
    }

    public Task InitializeAsync() => _db.StartAsync();

    public new async Task DisposeAsync()
    {
        await base.DisposeAsync();
        await _db.DisposeAsync();
    }
}

[CollectionDefinition(Name)]
public sealed class ApiCollection : ICollectionFixture<ApiFactory>
{
    public const string Name = "api";
}
