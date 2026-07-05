using Inscripciones.Domain.Models;

namespace Inscripciones.Domain.Ports;

/// <summary>
/// Puerto de salida (Hexagonal): contrato con el servicio Catálogo.
/// </summary>
public interface ICatalogoClient
{
    Task<MateriaCatalogoInfo?> ObtenerMateriaAsync(int materiaId, CancellationToken cancellationToken = default);
}
