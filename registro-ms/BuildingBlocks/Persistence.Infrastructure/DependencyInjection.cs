using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Persistence.Abstractions;
using Persistence.Infrastructure.Resilience;

namespace Persistence.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddDatabasePersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<DatabaseOptions>(configuration.GetSection(DatabaseOptions.SectionName));
        services.AddDatabaseResilience(configuration);
        return services;
    }
}
