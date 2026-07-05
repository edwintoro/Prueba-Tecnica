using Programas.Domain.Ports;
using Programas.Infrastructure.Persistence;
using Programas.Infrastructure.Persistence.Sql;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Programas.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddProgramasInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSingleton<ProgramaSqlDialectFactory>();
        services.AddScoped<IProgramaRepository, ProgramaRepository>();
        return services;
    }
}
