using Estudiantes.Domain.Ports;
using Estudiantes.Infrastructure.Persistence;
using Estudiantes.Infrastructure.Persistence.Sql;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Estudiantes.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddEstudiantesInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSingleton<EstudianteSqlDialectFactory>();
        services.AddScoped<IEstudianteRepository, EstudianteRepository>();
        return services;
    }
}
