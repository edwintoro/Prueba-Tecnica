using Auth.Application.Commands;
using Auth.Domain.Ports;
using Auth.Infrastructure.Persistence;
using Auth.Infrastructure.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Auth.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddAuthInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.Configure<AuthSessionOptions>(configuration.GetSection(AuthSessionOptions.SectionName));

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, HttpContextCurrentUser>();
        services.AddSingleton<IPasswordHasher, BcryptPasswordHasher>();
        services.AddSingleton<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        services.AddScoped<ISesionRepository, SesionRepository>();

        return services;
    }
}
