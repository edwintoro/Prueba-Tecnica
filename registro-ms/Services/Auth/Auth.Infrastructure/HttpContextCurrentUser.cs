using System.Security.Claims;
using Auth.Domain.Ports;
using Microsoft.AspNetCore.Http;

namespace Auth.Infrastructure;

public sealed class HttpContextCurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _http;

    public HttpContextCurrentUser(IHttpContextAccessor http) => _http = http;

    public int? UsuarioId => ParseInt(_http.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
        ?? _http.HttpContext?.User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value);

    public int? EstudianteId => ParseInt(_http.HttpContext?.User.FindFirst("estudiante_id")?.Value);

    public string? Rol => _http.HttpContext?.User.FindFirst(ClaimTypes.Role)?.Value;

    public Guid? SessionId => Guid.TryParse(_http.HttpContext?.User.FindFirst("sid")?.Value, out var sid) ? sid : null;

    public bool IsAuthenticated => _http.HttpContext?.User.Identity?.IsAuthenticated == true;

    private static int? ParseInt(string? value) =>
        int.TryParse(value, out var id) ? id : null;
}
