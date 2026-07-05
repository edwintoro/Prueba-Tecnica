using Auth.Domain.Ports;
using Auth.Infrastructure.Security;
using Persistence.Infrastructure.Resilience;

namespace RegistroMs.Api.Middleware;

public sealed class SessionActivityMiddleware
{
    private readonly RequestDelegate _next;

    public SessionActivityMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context, ISesionRepository sesiones)
    {
        await _next(context);

        if (context.User.Identity?.IsAuthenticated != true) return;
        var sid = context.User.FindFirst("sid")?.Value;
        if (!Guid.TryParse(sid, out var sessionId)) return;

        await sesiones.TouchAsync(sessionId, DateTime.UtcNow, context.RequestAborted);
    }
}

public sealed class DatabaseExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<DatabaseExceptionMiddleware> _logger;

    public DatabaseExceptionMiddleware(RequestDelegate next, ILogger<DatabaseExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (DatabaseUnavailableException ex)
        {
            _logger.LogWarning(ex, "Base de datos no disponible.");
            context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
            await context.Response.WriteAsJsonAsync(new { error = ex.Message });
        }
    }
}
