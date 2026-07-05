using Catalogo.Domain.Entities;
using Catalogo.Domain.ReadModels;

namespace Catalogo.Domain.Ports;

public interface IMateriaRepository
{
    Task<IReadOnlyList<MateriaConProfesor>> GetAllWithProfesorAsync(CancellationToken cancellationToken = default);
    Task<MateriaConProfesor?> GetByIdWithProfesorAsync(int id, CancellationToken cancellationToken = default);
    Task<int> CountByProfesorAsync(int profesorId, int? excludeMateriaId = null, CancellationToken cancellationToken = default);
    Task<int> CountAllAsync(CancellationToken cancellationToken = default);
    Task<int> AddAsync(Materia materia, CancellationToken cancellationToken = default);
    Task UpdateAsync(Materia materia, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<int?> GetActiveProgramaIdAsync(CancellationToken cancellationToken = default);
}
