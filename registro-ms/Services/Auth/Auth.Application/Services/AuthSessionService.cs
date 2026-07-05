using Auth.Application.Commands;
using Auth.Application.DTOs;
using Auth.Domain;
using Auth.Domain.Ports;
using Microsoft.Extensions.Options;

namespace Auth.Application.Services;

public sealed class AuthSessionService
{
    private readonly ISesionRepository _sesiones;
    private readonly IJwtTokenService _jwt;
    private readonly AuthSessionOptions _options;

    public AuthSessionService(
        ISesionRepository sesiones,
        IJwtTokenService jwt,
        IOptions<AuthSessionOptions> options)
    {
        _sesiones = sesiones;
        _jwt = jwt;
        _options = options.Value;
    }

    public async Task<AuthResponse> CreateSessionAsync(UsuarioAuth usuario, CancellationToken cancellationToken)
    {
        var (sessionId, refreshToken) = _jwt.GenerateRefreshToken();
        var now = DateTime.UtcNow;
        var sesion = new SesionAuth
        {
            Id = sessionId,
            UsuarioId = usuario.Id,
            RefreshTokenHash = _jwt.HashRefreshToken(refreshToken),
            UltimaActividad = now,
            ExpiraEn = now.AddHours(_options.RefreshTokenHours),
            Revocada = false
        };
        await _sesiones.CreateAsync(sesion, cancellationToken);

        var accessToken = _jwt.GenerateAccessToken(usuario, sessionId);
        return new AuthResponse(
            accessToken,
            refreshToken,
            sessionId,
            _options.AccessTokenMinutes * 60,
            usuario.RolCodigo,
            usuario.EstudianteId,
            ResolveNombre(usuario));
    }

    public static string? ResolveNombre(UsuarioAuth usuario) =>
        usuario.EstudianteNombre
        ?? (usuario.RolCodigo == RoleCodes.Administrador ? "Administrador" : usuario.Email);
}
