namespace Inscripciones.Domain.Models;

public sealed record ProgramaEstudianteInfo(
    int Id,
    string Nombre,
    int MaxMaterias,
    int TotalCreditos);

public sealed record MateriaCatalogoInfo(
    int Id,
    string Nombre,
    int Creditos,
    int ProfesorId,
    string ProfesorNombre,
    int ProgramaId);

public sealed record InscripcionDetalleInfo(
    int MateriaId,
    int Creditos,
    int ProfesorId);
