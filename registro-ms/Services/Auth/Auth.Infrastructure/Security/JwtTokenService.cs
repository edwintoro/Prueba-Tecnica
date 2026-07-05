using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Auth.Domain;
using Auth.Domain.Ports;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

namespace Auth.Infrastructure.Security;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";
    public string Key { get; set; } = "RegistroMs-Dev-Secret-Key-Min-32-Chars!";
    public string Issuer { get; set; } = "registro-ms";
    public string Audience { get; set; } = "registro-app";
}

public sealed class BcryptPasswordHasher : IPasswordHasher
{
    public string Hash(string password) => BCrypt.Net.BCrypt.HashPassword(password);
    public bool Verify(string password, string hash) => BCrypt.Net.BCrypt.Verify(password, hash);
}

public sealed class JwtTokenService : IJwtTokenService
{
    private readonly JwtOptions _jwt;
    private readonly Auth.Application.Commands.AuthSessionOptions _session;

    public JwtTokenService(
        IOptions<JwtOptions> jwt,
        IOptions<Auth.Application.Commands.AuthSessionOptions> session)
    {
        _jwt = jwt.Value;
        _session = session.Value;
    }

    public string GenerateAccessToken(UsuarioAuth usuario, Guid sessionId)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
            new(ClaimTypes.Role, usuario.RolCodigo),
            new("sid", sessionId.ToString())
        };
        if (usuario.EstudianteId.HasValue)
            claims.Add(new Claim("estudiante_id", usuario.EstudianteId.Value.ToString()));

        var token = new JwtSecurityToken(
            issuer: _jwt.Issuer,
            audience: _jwt.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_session.AccessTokenMinutes),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public (Guid SessionId, string RefreshToken) GenerateRefreshToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        return (Guid.NewGuid(), Convert.ToBase64String(bytes));
    }

    public string HashRefreshToken(string refreshToken)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken));
        return Convert.ToBase64String(bytes);
    }
}
