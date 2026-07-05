using Estudiantes.Domain.Entities;

namespace Estudiantes.Domain.Ports;

/// <summary>
/// Puerto de salida (Hexagonal): contrato de persistencia independiente del motor de BD.
/// </summary>
public interface IEstudianteRepository
{
    Task<IReadOnlyList<Estudiante>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Estudiante?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> ExistsByEmailAsync(string email, int? excludeId = null, CancellationToken cancellationToken = default);
    Task<int> AddAsync(Estudiante estudiante, CancellationToken cancellationToken = default);
    Task UpdateAsync(Estudiante estudiante, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
