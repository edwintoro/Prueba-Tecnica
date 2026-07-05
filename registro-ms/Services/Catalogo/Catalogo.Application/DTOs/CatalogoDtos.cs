namespace Catalogo.Application.DTOs;

public sealed record MateriaDto(
    int Id,
    string Nombre,
    int Creditos,
    int ProfesorId,
    string ProfesorNombre,
    int ProgramaId);

public sealed record ProfesorDto(
    int Id,
    string Nombre);

public sealed record CreateMateriaRequest(string Nombre, int Creditos, int ProfesorId, int? ProgramaId = null);
public sealed record UpdateMateriaRequest(string Nombre, int Creditos, int ProfesorId, int? ProgramaId = null);
public sealed record CreateProfesorRequest(string Nombre);
public sealed record UpdateProfesorRequest(string Nombre);
