using Auth.Application.Commands;
using Auth.Application.DTOs;
using Auth.Application.Services;
using Auth.Domain;
using Auth.Domain.Ports;
using MediatR;
using Microsoft.Extensions.Options;
using SharedKernel.Results;

namespace Auth.Application.Commands;

public sealed record LoginCommand(LoginRequest Request) : IRequest<Result<AuthResponse>>;
public sealed record RegisterEstudianteCommand(RegisterEstudianteRequest Request) : IRequest<Result<AuthResponse>>;
public sealed record RefreshTokenCommand(RefreshTokenRequest Request) : IRequest<Result<AuthResponse>>;
public sealed record LogoutCommand(Guid SessionId) : IRequest<Result>;

public sealed class LoginCommandHandler : IRequestHandler<LoginCommand, Result<AuthResponse>>
{
    private readonly IUsuarioRepository _usuarios;
    private readonly IPasswordHasher _hasher;
    private readonly AuthSessionService _sessions;

    public LoginCommandHandler(IUsuarioRepository usuarios, IPasswordHasher hasher, AuthSessionService sessions)
    {
        _usuarios = usuarios;
        _hasher = hasher;
        _sessions = sessions;
    }

    public async Task<Result<AuthResponse>> Handle(LoginCommand command, CancellationToken cancellationToken)
    {
        var req = command.Request;
        var usuario = await _usuarios.GetByEmailAsync(req.Email.Trim().ToLowerInvariant(), cancellationToken);
        if (usuario is null || !usuario.Activo || !_hasher.Verify(req.Password, usuario.PasswordHash))
            return Result.Failure<AuthResponse>("Credenciales inválidas.");

        if (!RoleCodes.CanAccessApp(usuario.RolCodigo))
            return Result.Failure<AuthResponse>("Rol no autorizado para acceder a esta aplicación.");

        var response = await _sessions.CreateSessionAsync(usuario, cancellationToken);
        return Result.Success(response);
    }
}

public sealed class RegisterEstudianteCommandHandler : IRequestHandler<RegisterEstudianteCommand, Result<AuthResponse>>
{
    private readonly IUsuarioRepository _usuarios;
    private readonly IPasswordHasher _hasher;
    private readonly AuthSessionService _sessions;

    public RegisterEstudianteCommandHandler(
        IUsuarioRepository usuarios,
        IPasswordHasher hasher,
        AuthSessionService sessions)
    {
        _usuarios = usuarios;
        _hasher = hasher;
        _sessions = sessions;
    }

    public async Task<Result<AuthResponse>> Handle(RegisterEstudianteCommand command, CancellationToken cancellationToken)
    {
        var req = command.Request;
        var email = req.Email.Trim().ToLowerInvariant();
        if (await _usuarios.ExistsByEmailAsync(email, cancellationToken))
            return Result.Failure<AuthResponse>("Ya existe un usuario con este email.");

        var hash = _hasher.Hash(req.Password);
        await _usuarios.CreateEstudianteUsuarioAsync(email, hash, req.Nombre.Trim(), cancellationToken);

        var usuario = await _usuarios.GetByEmailAsync(email, cancellationToken);
        return usuario is null
            ? Result.Failure<AuthResponse>("No se pudo completar el registro.")
            : Result.Success(await _sessions.CreateSessionAsync(usuario, cancellationToken));
    }
}

public sealed class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, Result<AuthResponse>>
{
    private readonly IUsuarioRepository _usuarios;
    private readonly ISesionRepository _sesiones;
    private readonly IJwtTokenService _jwt;
    private readonly AuthSessionOptions _options;

    public RefreshTokenCommandHandler(
        IUsuarioRepository usuarios,
        ISesionRepository sesiones,
        IJwtTokenService jwt,
        IOptions<AuthSessionOptions> options)
    {
        _usuarios = usuarios;
        _sesiones = sesiones;
        _jwt = jwt;
        _options = options.Value;
    }

    public async Task<Result<AuthResponse>> Handle(RefreshTokenCommand command, CancellationToken cancellationToken)
    {
        var sesion = await _sesiones.GetByIdAsync(command.Request.SessionId, cancellationToken);
        if (sesion is null || sesion.Revocada)
            return Result.Failure<AuthResponse>("Sesión inválida.");

        var now = DateTime.UtcNow;
        if ((now - sesion.UltimaActividad).TotalMinutes > _options.InactivityMinutes)
        {
            await _sesiones.RevocarAsync(sesion.Id, cancellationToken);
            return Result.Failure<AuthResponse>("Sesión expirada por inactividad.");
        }

        if (sesion.RefreshTokenHash != _jwt.HashRefreshToken(command.Request.RefreshToken))
            return Result.Failure<AuthResponse>("Token de refresco inválido.");

        var usuario = await _usuarios.GetByIdAsync(sesion.UsuarioId, cancellationToken);
        if (usuario is null || !usuario.Activo || !RoleCodes.CanAccessApp(usuario.RolCodigo))
            return Result.Failure<AuthResponse>("Usuario no autorizado.");

        var (_, newRefresh) = _jwt.GenerateRefreshToken();
        await _sesiones.UpdateActividadAsync(
            sesion.Id,
            now,
            _jwt.HashRefreshToken(newRefresh),
            now.AddHours(_options.RefreshTokenHours),
            cancellationToken);

        var accessToken = _jwt.GenerateAccessToken(usuario, sesion.Id);
        return Result.Success(new AuthResponse(
            accessToken,
            newRefresh,
            sesion.Id,
            _options.AccessTokenMinutes * 60,
            usuario.RolCodigo,
            usuario.EstudianteId,
            AuthSessionService.ResolveNombre(usuario)));
    }
}

public sealed class LogoutCommandHandler : IRequestHandler<LogoutCommand, Result>
{
    private readonly ISesionRepository _sesiones;

    public LogoutCommandHandler(ISesionRepository sesiones) => _sesiones = sesiones;

    public async Task<Result> Handle(LogoutCommand command, CancellationToken cancellationToken)
    {
        await _sesiones.RevocarAsync(command.SessionId, cancellationToken);
        return Result.Success();
    }
}

public sealed class AuthSessionOptions
{
    public const string SectionName = "Auth";
    public int AccessTokenMinutes { get; set; } = 5;
    public int InactivityMinutes { get; set; } = 5;
    public int RefreshTokenHours { get; set; } = 8;
}
