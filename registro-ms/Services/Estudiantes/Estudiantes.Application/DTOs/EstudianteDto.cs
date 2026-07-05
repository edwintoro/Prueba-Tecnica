namespace Estudiantes.Application.DTOs;

public sealed record EstudianteDto(
    int Id,
    string Nombre,
    string Email,
    DateTime FechaRegistro);

public sealed record CreateEstudianteRequest(string Nombre, string Email);
public sealed record UpdateEstudianteRequest(string Nombre, string Email);
