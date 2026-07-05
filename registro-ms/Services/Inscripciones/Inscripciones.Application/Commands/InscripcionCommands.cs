using Inscripciones.Application.DTOs;
using Inscripciones.Application.Queries;
using Inscripciones.Domain.Entities;
using Inscripciones.Domain.Models;
using Inscripciones.Domain.Ports;
using Inscripciones.Domain.Services;
using MediatR;
using SharedKernel.Results;

namespace Inscripciones.Application.Commands;

public sealed record InscribirMateriaCommand(InscribirMateriaRequest Request) : IRequest<Result<InscripcionDto>>;

public sealed class InscribirMateriaCommandHandler : IRequestHandler<InscribirMateriaCommand, Result<InscripcionDto>>
{
    private readonly IInscripcionRepository _repository;
    private readonly IProgramasClient _programasClient;
    private readonly ICatalogoClient _catalogoClient;
    private readonly InscripcionDomainService _domainService;

    public InscribirMateriaCommandHandler(
        IInscripcionRepository repository,
        IProgramasClient programasClient,
        ICatalogoClient catalogoClient,
        InscripcionDomainService domainService)
    {
        _repository = repository;
        _programasClient = programasClient;
        _catalogoClient = catalogoClient;
        _domainService = domainService;
    }

    public async Task<Result<InscripcionDto>> Handle(InscribirMateriaCommand command, CancellationToken cancellationToken)
    {
        var req = command.Request;

        if (!await _programasClient.EstaAdheridoAsync(req.EstudianteId, cancellationToken))
            return Result.Failure<InscripcionDto>("Debe adherirse al programa de créditos antes de inscribir materias.");

        var programa = await _programasClient.ObtenerProgramaDelEstudianteAsync(req.EstudianteId, cancellationToken);
        if (programa is null)
            return Result.Failure<InscripcionDto>("No se encontró el programa de créditos del estudiante.");

        var materiaNueva = await _catalogoClient.ObtenerMateriaAsync(req.MateriaId, cancellationToken);
        if (materiaNueva is null)
            return Result.Failure<InscripcionDto>("La materia seleccionada no existe.");

        var inscripciones = await _repository.GetByEstudianteIdAsync(req.EstudianteId, cancellationToken);
        var detalles = new List<InscripcionDetalleInfo>(inscripciones.Count);

        foreach (var inscripcion in inscripciones)
        {
            var materia = await _catalogoClient.ObtenerMateriaAsync(inscripcion.MateriaId, cancellationToken);
            if (materia is null)
                return Result.Failure<InscripcionDto>($"No se encontró la materia con id {inscripcion.MateriaId}.");

            detalles.Add(GetInscripcionesByEstudianteQueryHandler.ToDetalle(materia));
        }

        var validation = _domainService.ValidarInscripcion(detalles, materiaNueva, programa);
        if (validation.IsFailure)
            return Result.Failure<InscripcionDto>(validation.Error!);

        var entity = Inscripcion.Create(req.EstudianteId, req.MateriaId);
        var id = await _repository.AddAsync(entity, cancellationToken);
        var created = Inscripcion.Rehydrate(id, entity.EstudianteId, entity.MateriaId, entity.FechaInscripcion);

        return Result.Success(GetInscripcionesByEstudianteQueryHandler.Map(created, materiaNueva));
    }
}

public sealed record CancelarInscripcionCommand(int Id) : IRequest<Result>;

public sealed class CancelarInscripcionCommandHandler : IRequestHandler<CancelarInscripcionCommand, Result>
{
    private readonly IInscripcionRepository _repository;

    public CancelarInscripcionCommandHandler(IInscripcionRepository repository) => _repository = repository;

    public async Task<Result> Handle(CancelarInscripcionCommand command, CancellationToken cancellationToken)
    {
        var inscripcion = await _repository.GetByIdAsync(command.Id, cancellationToken);
        if (inscripcion is null)
            return Result.Failure("Inscripción no encontrada.");

        await _repository.DeleteAsync(command.Id, cancellationToken);
        return Result.Success();
    }
}
