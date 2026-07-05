using SharedKernel.Primitives;

namespace Inscripciones.Domain.Entities;

public sealed class Inscripcion : Entity<int>
{
    public int EstudianteId { get; private set; }
    public int MateriaId { get; private set; }
    public DateTime FechaInscripcion { get; private set; }

    private Inscripcion() { }

    public static Inscripcion Create(int estudianteId, int materiaId)
    {
        if (estudianteId <= 0)
            throw new ArgumentOutOfRangeException(nameof(estudianteId));
        if (materiaId <= 0)
            throw new ArgumentOutOfRangeException(nameof(materiaId));

        return new Inscripcion
        {
            EstudianteId = estudianteId,
            MateriaId = materiaId,
            FechaInscripcion = DateTime.UtcNow
        };
    }

    public static Inscripcion Rehydrate(int id, int estudianteId, int materiaId, DateTime fechaInscripcion) =>
        new()
        {
            Id = id,
            EstudianteId = estudianteId,
            MateriaId = materiaId,
            FechaInscripcion = fechaInscripcion
        };
}
