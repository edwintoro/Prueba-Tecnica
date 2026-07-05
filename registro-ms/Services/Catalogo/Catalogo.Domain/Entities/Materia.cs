using SharedKernel.Primitives;

namespace Catalogo.Domain.Entities;

public sealed class Materia : Entity<int>
{
    public const int MaxMateriasPorProfesor = 2;
    public const int MaxMateriasEnCatalogo = 10;

    public string Nombre { get; private set; } = string.Empty;
    public int Creditos { get; private set; }
    public int ProfesorId { get; private set; }
    public int ProgramaId { get; private set; }

    private Materia() { }

    public static Materia Create(string nombre, int creditos, int profesorId, int programaId) =>
        new()
        {
            Nombre = nombre.Trim(),
            Creditos = creditos,
            ProfesorId = profesorId,
            ProgramaId = programaId
        };

    public static Materia Rehydrate(int id, string nombre, int creditos, int profesorId, int programaId) =>
        new()
        {
            Id = id,
            Nombre = nombre,
            Creditos = creditos,
            ProfesorId = profesorId,
            ProgramaId = programaId
        };

    public void Update(string nombre, int creditos, int profesorId, int programaId)
    {
        Nombre = nombre.Trim();
        Creditos = creditos;
        ProfesorId = profesorId;
        ProgramaId = programaId;
    }
}
