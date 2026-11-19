using HypertrophyApp.Application.Common;
using HypertrophyApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HypertrophyApp.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("HypertrophyApp")
            ?? throw new InvalidOperationException("Connection string 'HypertrophyApp' is not configured.");

        services.AddDbContext<HypertrophyAppDbContext>(o => o.UseNpgsql(connectionString));
        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<HypertrophyAppDbContext>());
        return services;
    }
}
