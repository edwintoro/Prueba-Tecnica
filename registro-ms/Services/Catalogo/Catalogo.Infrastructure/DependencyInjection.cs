using Catalogo.Domain.Ports;
using Catalogo.Infrastructure.Persistence;
using Catalogo.Infrastructure.Persistence.Sql;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Catalogo.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddCatalogoInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSingleton<ProfesorSqlDialectFactory>();
        services.AddSingleton<MateriaSqlDialectFactory>();
        services.AddScoped<IProfesorRepository, ProfesorRepository>();
        services.AddScoped<IMateriaRepository, MateriaRepository>();
        return services;
    }
}
