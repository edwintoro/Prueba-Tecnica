using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Programas.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddProgramasApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
        return services;
    }
}
