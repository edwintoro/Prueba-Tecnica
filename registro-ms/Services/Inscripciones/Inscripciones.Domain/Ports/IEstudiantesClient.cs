namespace Inscripciones.Domain.Ports;

/// <summary>
/// Puerto de salida hacia el módulo Estudiantes (in-process en registro-ms).
/// </summary>
public interface IEstudiantesClient
{
    Task<string?> ObtenerNombreAsync(int estudianteId, CancellationToken cancellationToken = default);
}
