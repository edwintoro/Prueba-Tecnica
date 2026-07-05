using Programas.Application.DTOs;
using Programas.Domain.Entities;
using Programas.Domain.Ports;
using MediatR;
using SharedKernel.Results;

namespace Programas.Application.Commands;

public sealed record AdherirEstudianteCommand(AdherirEstudianteRequest Request) : IRequest<Result<AdhesionEstudianteDto>>;

public sealed class AdherirEstudianteCommandHandler : IRequestHandler<AdherirEstudianteCommand, Result<AdhesionEstudianteDto>>
{
    private readonly IProgramaRepository _repository;

    public AdherirEstudianteCommandHandler(IProgramaRepository repository) => _repository = repository;

    public async Task<Result<AdhesionEstudianteDto>> Handle(AdherirEstudianteCommand command, CancellationToken cancellationToken)
    {
        var req = command.Request;

        if (await _repository.IsEstudianteAdheridoAsync(req.EstudianteId, cancellationToken))
            return Result.Failure<AdhesionEstudianteDto>("El estudiante ya está adherido al programa de créditos.");

        var programaActivo = await _repository.GetActivoAsync(cancellationToken);
        if (programaActivo is null)
            return Result.Failure<AdhesionEstudianteDto>("No hay un programa de créditos activo.");

        if (programaActivo.Id != req.ProgramaId)
            return Result.Failure<AdhesionEstudianteDto>("El programa indicado no es el programa activo.");

        var adhesion = EstudiantePrograma.Create(req.EstudianteId, req.ProgramaId);
        await _repository.AdherirEstudianteAsync(adhesion, cancellationToken);

        return Result.Success(new AdhesionEstudianteDto(true, adhesion.ProgramaId, adhesion.FechaAdhesion));
    }
}
