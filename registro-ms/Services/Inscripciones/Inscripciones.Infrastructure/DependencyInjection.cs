using Inscripciones.Domain.Ports;
using Inscripciones.Infrastructure.Persistence;
using Inscripciones.Infrastructure.Persistence.Sql;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Inscripciones.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInscripcionesInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSingleton<InscripcionSqlDialectFactory>();
        services.AddScoped<IInscripcionRepository, InscripcionRepository>();
        return services;
    }
}
