using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Estudiantes.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddEstudiantesApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
        return services;
    }
}
