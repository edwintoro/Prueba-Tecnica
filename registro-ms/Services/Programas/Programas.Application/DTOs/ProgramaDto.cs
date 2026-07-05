namespace Programas.Application.DTOs;

public sealed record ProgramaDto(
    int Id,
    string Nombre,
    int CreditosPorMateria,
    int MaxMaterias,
    int TotalCreditos,
    bool Activo);

public sealed record AdhesionEstudianteDto(
    bool EstaAdherido,
    int? ProgramaId,
    DateTime? FechaAdhesion);

public sealed record AdherirEstudianteRequest(int EstudianteId, int ProgramaId);
