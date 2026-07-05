using Auth.Domain;

namespace Auth.Domain.Ports;

public interface IUsuarioRepository
{
    Task<UsuarioAuth?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<UsuarioAuth?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<int> CreateEstudianteUsuarioAsync(string email, string passwordHash, string nombreEstudiante, CancellationToken cancellationToken = default);
    Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default);
}

public interface ISesionRepository
{
    Task CreateAsync(SesionAuth sesion, CancellationToken cancellationToken = default);
    Task<SesionAuth?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task UpdateActividadAsync(Guid id, DateTime ultimaActividad, string refreshTokenHash, DateTime expiraEn, CancellationToken cancellationToken = default);
    Task TouchAsync(Guid id, DateTime ultimaActividad, CancellationToken cancellationToken = default);
    Task RevocarAsync(Guid id, CancellationToken cancellationToken = default);
}

public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string hash);
}

public interface IJwtTokenService
{
    string GenerateAccessToken(UsuarioAuth usuario, Guid sessionId);
    (Guid SessionId, string RefreshToken) GenerateRefreshToken();
    string HashRefreshToken(string refreshToken);
}

public interface ICurrentUser
{
    int? UsuarioId { get; }
    int? EstudianteId { get; }
    string? Rol { get; }
    Guid? SessionId { get; }
    bool IsAuthenticated { get; }
}
