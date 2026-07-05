using Inscripciones.Application.DTOs;
using Inscripciones.Domain.Entities;
using Inscripciones.Domain.Models;
using Inscripciones.Domain.Ports;
using MediatR;
using SharedKernel.Results;

namespace Inscripciones.Application.Queries;

public sealed record GetInscripcionesByEstudianteQuery(int EstudianteId)
    : IRequest<Result<IReadOnlyList<InscripcionDto>>>;

public sealed class GetInscripcionesByEstudianteQueryHandler
    : IRequestHandler<GetInscripcionesByEstudianteQuery, Result<IReadOnlyList<InscripcionDto>>>
{
    private readonly IInscripcionRepository _repository;
    private readonly ICatalogoClient _catalogoClient;

    public GetInscripcionesByEstudianteQueryHandler(
        IInscripcionRepository repository,
        ICatalogoClient catalogoClient)
    {
        _repository = repository;
        _catalogoClient = catalogoClient;
    }

    public async Task<Result<IReadOnlyList<InscripcionDto>>> Handle(
        GetInscripcionesByEstudianteQuery request,
        CancellationToken cancellationToken)
    {
        var inscripciones = await _repository.GetByEstudianteIdAsync(request.EstudianteId, cancellationToken);
        var dtos = new List<InscripcionDto>(inscripciones.Count);

        foreach (var inscripcion in inscripciones)
        {
            var materia = await _catalogoClient.ObtenerMateriaAsync(inscripcion.MateriaId, cancellationToken);
            if (materia is null)
                return Result.Failure<IReadOnlyList<InscripcionDto>>(
                    $"No se encontró la materia con id {inscripcion.MateriaId}.");

            dtos.Add(Map(inscripcion, materia));
        }

        return Result.Success<IReadOnlyList<InscripcionDto>>(dtos);
    }

    internal static InscripcionDto Map(Inscripcion inscripcion, MateriaCatalogoInfo materia) =>
        new(
            inscripcion.Id,
            inscripcion.EstudianteId,
            inscripcion.MateriaId,
            materia.Nombre,
            materia.Creditos,
            materia.ProfesorId,
            materia.ProfesorNombre,
            inscripcion.FechaInscripcion);

    internal static InscripcionDetalleInfo ToDetalle(MateriaCatalogoInfo materia) =>
        new(materia.Id, materia.Creditos, materia.ProfesorId);
}
