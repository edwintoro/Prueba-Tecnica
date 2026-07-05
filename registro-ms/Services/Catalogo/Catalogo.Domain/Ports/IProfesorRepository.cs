using Catalogo.Domain.Entities;

namespace Catalogo.Domain.Ports;

public interface IProfesorRepository
{
    Task<IReadOnlyList<Profesor>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Profesor?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<int> AddAsync(Profesor profesor, CancellationToken cancellationToken = default);
    Task UpdateAsync(Profesor profesor, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> HasMateriasAsync(int id, CancellationToken cancellationToken = default);
}
