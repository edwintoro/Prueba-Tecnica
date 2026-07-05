namespace Auth.Domain;

public static class RoleCodes
{
    public const string Estudiante = "Estudiante";
    public const string Administrador = "Administrador";
    public const string Profesor = "Profesor";
    public const string Secretaria = "Secretaria";

    public static bool CanAccessApp(string rolCodigo) =>
        rolCodigo is Estudiante or Administrador;
}

public sealed class UsuarioAuth
{
    public int Id { get; init; }
    public string Email { get; init; } = string.Empty;
    public string PasswordHash { get; init; } = string.Empty;
    public string RolCodigo { get; init; } = string.Empty;
    public int? EstudianteId { get; init; }
    public string? EstudianteNombre { get; init; }
    public bool Activo { get; init; }
}

public sealed class SesionAuth
{
    public Guid Id { get; init; }
    public int UsuarioId { get; init; }
    public string RefreshTokenHash { get; init; } = string.Empty;
    public DateTime UltimaActividad { get; init; }
    public DateTime ExpiraEn { get; init; }
    public bool Revocada { get; init; }
}
