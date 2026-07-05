namespace Inscripciones.Application.DTOs;

public sealed record InscripcionDto(
    int Id,
    int EstudianteId,
    int MateriaId,
    string MateriaNombre,
    int Creditos,
    int ProfesorId,
    string ProfesorNombre,
    DateTime FechaInscripcion);

public sealed record InscribirMateriaRequest(int EstudianteId, int MateriaId);
