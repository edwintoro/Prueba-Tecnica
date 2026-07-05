using Catalogo.Domain.Ports;
using Estudiantes.Domain.Ports;
using Inscripciones.Domain.Models;
using Inscripciones.Domain.Ports;
using Programas.Domain.Ports;

namespace RegistroMs.Api.Adapters;

/// <summary>
/// Adaptadores in-process entre módulos del microservicio registro-ms (hexagonal).
/// </summary>
public sealed class ProgramasModuleAdapter : IProgramasClient
{
    private readonly IProgramaRepository _repository;

    public ProgramasModuleAdapter(IProgramaRepository repository) => _repository = repository;

    public async Task<bool> EstaAdheridoAsync(int estudianteId, CancellationToken cancellationToken = default) =>
        await _repository.IsEstudianteAdheridoAsync(estudianteId, cancellationToken);

    public async Task<ProgramaEstudianteInfo?> ObtenerProgramaDelEstudianteAsync(
        int estudianteId, CancellationToken cancellationToken = default)
    {
        if (!await EstaAdheridoAsync(estudianteId, cancellationToken))
            return null;

        var programa = await _repository.GetActivoAsync(cancellationToken);
        return programa is null
            ? null
            : new ProgramaEstudianteInfo(programa.Id, programa.Nombre, programa.MaxMaterias, programa.TotalCreditos);
    }
}

public sealed class CatalogoModuleAdapter : ICatalogoClient
{
    private readonly IMateriaRepository _repository;

    public CatalogoModuleAdapter(IMateriaRepository repository) => _repository = repository;

    public async Task<MateriaCatalogoInfo?> ObtenerMateriaAsync(int materiaId, CancellationToken cancellationToken = default)
    {
        var materia = await _repository.GetByIdWithProfesorAsync(materiaId, cancellationToken);
        return materia is null
            ? null
            : new MateriaCatalogoInfo(
                materia.Id, materia.Nombre, materia.Creditos,
                materia.ProfesorId, materia.ProfesorNombre, materia.ProgramaId);
    }
}

public sealed class EstudiantesModuleAdapter : IEstudiantesClient
{
    private readonly IEstudianteRepository _repository;

    public EstudiantesModuleAdapter(IEstudianteRepository repository) => _repository = repository;

    public async Task<string?> ObtenerNombreAsync(int estudianteId, CancellationToken cancellationToken = default)
    {
        var estudiante = await _repository.GetByIdAsync(estudianteId, cancellationToken);
        return estudiante?.Nombre;
    }
}
