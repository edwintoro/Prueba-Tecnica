namespace Blazor.Shared.Auth;

public sealed record AuthSession(
    string AccessToken,
    string RefreshToken,
    Guid SessionId,
    int ExpiresInSeconds,
    string Rol,
    int? EstudianteId,
    string? Nombre);

public sealed record LoginRequest(string Email, string Password);
public sealed record RegisterRequest(string Nombre, string Email, string Password);
public sealed record RefreshRequest(string RefreshToken, Guid SessionId);
public sealed record MeResponse(int UsuarioId, string Email, string Rol, int? EstudianteId, string? Nombre);
