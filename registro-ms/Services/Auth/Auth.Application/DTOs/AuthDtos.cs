namespace Auth.Application.DTOs;

public sealed record LoginRequest(string Email, string Password);
public sealed record RegisterEstudianteRequest(string Nombre, string Email, string Password);
public sealed record RefreshTokenRequest(string RefreshToken, Guid SessionId);
public sealed record AuthResponse(
    string AccessToken,
    string RefreshToken,
    Guid SessionId,
    int ExpiresInSeconds,
    string Rol,
    int? EstudianteId,
    string? Nombre);
public sealed record MeResponse(int UsuarioId, string Email, string Rol, int? EstudianteId, string? Nombre);
