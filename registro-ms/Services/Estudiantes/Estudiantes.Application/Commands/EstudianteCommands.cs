using Estudiantes.Application.DTOs;
using Estudiantes.Application.Queries;
using Estudiantes.Domain.Entities;
using Estudiantes.Domain.Ports;
using MediatR;
using SharedKernel.Results;

namespace Estudiantes.Application.Commands;

public sealed record CreateEstudianteCommand(CreateEstudianteRequest Request) : IRequest<Result<EstudianteDto>>;

public sealed class CreateEstudianteCommandHandler : IRequestHandler<CreateEstudianteCommand, Result<EstudianteDto>>
{
    private readonly IEstudianteRepository _repository;

    public CreateEstudianteCommandHandler(IEstudianteRepository repository) => _repository = repository;

    public async Task<Result<EstudianteDto>> Handle(CreateEstudianteCommand command, CancellationToken cancellationToken)
    {
        var req = command.Request;
        if (await _repository.ExistsByEmailAsync(req.Email, cancellationToken: cancellationToken))
            return Result.Failure<EstudianteDto>("Ya existe un estudiante con este email.");

        var estudiante = Estudiante.Create(req.Nombre, req.Email);
        var id = await _repository.AddAsync(estudiante, cancellationToken);
        var created = Estudiante.Rehydrate(id, estudiante.Nombre, estudiante.Email, estudiante.FechaRegistro);
        return Result.Success(GetEstudiantesQueryHandler.Map(created));
    }
}

public sealed record UpdateEstudianteCommand(int Id, UpdateEstudianteRequest Request) : IRequest<Result<EstudianteDto>>;

public sealed class UpdateEstudianteCommandHandler : IRequestHandler<UpdateEstudianteCommand, Result<EstudianteDto>>
{
    private readonly IEstudianteRepository _repository;

    public UpdateEstudianteCommandHandler(IEstudianteRepository repository) => _repository = repository;

    public async Task<Result<EstudianteDto>> Handle(UpdateEstudianteCommand command, CancellationToken cancellationToken)
    {
        var estudiante = await _repository.GetByIdAsync(command.Id, cancellationToken);
        if (estudiante is null)
            return Result.Failure<EstudianteDto>("Estudiante no encontrado.");

        if (await _repository.ExistsByEmailAsync(command.Request.Email, command.Id, cancellationToken))
            return Result.Failure<EstudianteDto>("Ya existe un estudiante con este email.");

        estudiante.Update(command.Request.Nombre, command.Request.Email);
        await _repository.UpdateAsync(estudiante, cancellationToken);
        return Result.Success(GetEstudiantesQueryHandler.Map(estudiante));
    }
}

public sealed record DeleteEstudianteCommand(int Id) : IRequest<Result>;

public sealed class DeleteEstudianteCommandHandler : IRequestHandler<DeleteEstudianteCommand, Result>
{
    private readonly IEstudianteRepository _repository;

    public DeleteEstudianteCommandHandler(IEstudianteRepository repository) => _repository = repository;

    public async Task<Result> Handle(DeleteEstudianteCommand command, CancellationToken cancellationToken)
    {
        var estudiante = await _repository.GetByIdAsync(command.Id, cancellationToken);
        if (estudiante is null)
            return Result.Failure("Estudiante no encontrado.");

        await _repository.DeleteAsync(command.Id, cancellationToken);
        return Result.Success();
    }
}
