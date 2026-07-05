using System.Reflection;
using Inscripciones.Domain.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Inscripciones.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddInscripcionesApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
        services.AddScoped<InscripcionDomainService>();
        return services;
    }
}
