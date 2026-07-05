namespace Catalogo.Domain.ReadModels;

public sealed class MateriaConProfesor
{
    public int Id { get; init; }
    public string Nombre { get; init; } = string.Empty;
    public int Creditos { get; init; }
    public int ProfesorId { get; init; }
    public string ProfesorNombre { get; init; } = string.Empty;
    public int ProgramaId { get; init; }

    public static MateriaConProfesor Rehydrate(
        int id,
        string nombre,
        int creditos,
        int profesorId,
        string profesorNombre,
        int programaId) =>
        new()
        {
            Id = id,
            Nombre = nombre,
            Creditos = creditos,
            ProfesorId = profesorId,
            ProfesorNombre = profesorNombre,
            ProgramaId = programaId
        };
}
