using Blazor.Shared.Auth;
using Blazor.Shared.Options;
using Blazor.Shared.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Blazor.Shared;

public static class DependencyInjection
{
    public static IServiceCollection AddBlazorSharedServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<ApiGatewayOptions>(configuration.GetSection(ApiGatewayOptions.SectionName));
        services.AddSingleton<ITokenStorage, TokenStorage>();
        services.AddSingleton<AuthBridgeService>();

        services.AddHttpClient<IAuthApiClient, AuthApiClient>();

        services.AddTransient<AuthMessageHandler>();
        services.AddHttpClient<IEstudiantesApiClient, EstudiantesApiClient>()
            .AddHttpMessageHandler<AuthMessageHandler>();
        services.AddHttpClient<IProgramasApiClient, ProgramasApiClient>()
            .AddHttpMessageHandler<AuthMessageHandler>();
        services.AddHttpClient<ICatalogoApiClient, CatalogoApiClient>()
            .AddHttpMessageHandler<AuthMessageHandler>();
        services.AddHttpClient<IInscripcionesApiClient, InscripcionesApiClient>()
            .AddHttpMessageHandler<AuthMessageHandler>();

        return services;
    }

    public static IServiceCollection AddBlazorAuthOnlyServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<ApiGatewayOptions>(configuration.GetSection(ApiGatewayOptions.SectionName));
        services.AddHttpClient<IAuthApiClient, AuthApiClient>();
        return services;
    }
}
