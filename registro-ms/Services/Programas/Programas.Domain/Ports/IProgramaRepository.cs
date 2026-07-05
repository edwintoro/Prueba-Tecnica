using Programas.Domain.Entities;

namespace Programas.Domain.Ports;

/// <summary>
/// Puerto de salida (Hexagonal): contrato de persistencia independiente del motor de BD.
/// </summary>
public interface IProgramaRepository
{
    Task<ProgramaCredito?> GetActivoAsync(CancellationToken cancellationToken = default);
    Task<EstudiantePrograma?> GetAdhesionByEstudianteIdAsync(int estudianteId, CancellationToken cancellationToken = default);
    Task<bool> IsEstudianteAdheridoAsync(int estudianteId, CancellationToken cancellationToken = default);
    Task<int> AdherirEstudianteAsync(EstudiantePrograma adhesion, CancellationToken cancellationToken = default);
}
