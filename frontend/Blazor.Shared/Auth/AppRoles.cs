namespace Blazor.Shared.Auth;

public static class AppRoles
{
    public const string Estudiante = "Estudiante";
    public const string Administrador = "Administrador";

    public static bool IsAdministrador(string? rol) =>
        string.Equals(rol, Administrador, StringComparison.OrdinalIgnoreCase);

    public static bool IsEstudiante(string? rol) =>
        string.Equals(rol, Estudiante, StringComparison.OrdinalIgnoreCase);

    public static string Display(string? rol) => rol switch
    {
        Administrador => "Administrador",
        Estudiante => "Estudiante",
        _ => rol ?? "Usuario"
    };
}
