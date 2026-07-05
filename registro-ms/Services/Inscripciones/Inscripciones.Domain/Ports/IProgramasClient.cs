using Inscripciones.Domain.Models;

namespace Inscripciones.Domain.Ports;

/// <summary>
/// Puerto de salida (Hexagonal): contrato con el servicio Programas.
/// </summary>
public interface IProgramasClient
{
    Task<bool> EstaAdheridoAsync(int estudianteId, CancellationToken cancellationToken = default);
    Task<ProgramaEstudianteInfo?> ObtenerProgramaDelEstudianteAsync(int estudianteId, CancellationToken cancellationToken = default);
}
