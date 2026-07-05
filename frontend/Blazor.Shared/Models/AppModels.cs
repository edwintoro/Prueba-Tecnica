namespace Blazor.Shared.Models;

public sealed record EstudianteModel(int Id, string Nombre, string Email, DateTime FechaRegistro);
public sealed record CreateEstudianteModel(string Nombre, string Email);
public sealed record UpdateEstudianteModel(string Nombre, string Email);

public sealed record ProgramaModel(
    int Id, string Nombre, int CreditosPorMateria, int MaxMaterias, int TotalCreditos, bool Activo);

public sealed record AdhesionModel(bool EstaAdherido, int? ProgramaId, DateTime? FechaAdhesion);
public sealed record AdherirRequest(int EstudianteId, int ProgramaId);

public sealed record MateriaModel(
    int Id, string Nombre, int Creditos, int ProfesorId, string ProfesorNombre, int ProgramaId);
public sealed record CreateMateriaModel(string Nombre, int Creditos, int ProfesorId);
public sealed record UpdateMateriaModel(string Nombre, int Creditos, int ProfesorId);

public sealed record ProfesorModel(int Id, string Nombre);
public sealed record CreateProfesorModel(string Nombre);
public sealed record UpdateProfesorModel(string Nombre);

public sealed record InscripcionModel(
    int Id, int EstudianteId, int MateriaId, string MateriaNombre,
    int Creditos, int ProfesorId, string ProfesorNombre, DateTime FechaInscripcion);

public sealed record InscribirRequest(int EstudianteId, int MateriaId);
