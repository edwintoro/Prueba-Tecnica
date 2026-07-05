using SharedKernel.Primitives;

namespace Programas.Domain.Entities;

public sealed class EstudiantePrograma : Entity<int>
{
    public int EstudianteId { get; private set; }
    public int ProgramaId { get; private set; }
    public DateTime FechaAdhesion { get; private set; }

    private EstudiantePrograma() { }

    public static EstudiantePrograma Create(int estudianteId, int programaId) =>
        new()
        {
            EstudianteId = estudianteId,
            ProgramaId = programaId,
            FechaAdhesion = DateTime.UtcNow
        };

    public static EstudiantePrograma Rehydrate(int id, int estudianteId, int programaId, DateTime fechaAdhesion) =>
        new()
        {
            Id = id,
            EstudianteId = estudianteId,
            ProgramaId = programaId,
            FechaAdhesion = fechaAdhesion
        };
}
