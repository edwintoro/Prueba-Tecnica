using Inscripciones.Domain.Entities;

namespace Inscripciones.Domain.Ports;

/// <summary>
/// Puerto de salida (Hexagonal): contrato de persistencia independiente del motor de BD.
/// </summary>
public interface IInscripcionRepository
{
    Task<IReadOnlyList<Inscripcion>> GetByEstudianteIdAsync(int estudianteId, CancellationToken cancellationToken = default);
    Task<Inscripcion?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> ExistsByEstudianteAndMateriaAsync(int estudianteId, int materiaId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<int>> GetEstudianteIdsByMateriaAsync(int materiaId, int? excludeEstudianteId = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> GetCompanerosNombresAsync(int materiaId, int excludeEstudianteId, CancellationToken cancellationToken = default);
    Task<int> AddAsync(Inscripcion inscripcion, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
