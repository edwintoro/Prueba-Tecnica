using Estudiantes.Application.DTOs;
using Estudiantes.Domain.Entities;
using Estudiantes.Domain.Ports;
using MediatR;
using SharedKernel.Results;

namespace Estudiantes.Application.Queries;

public sealed record GetEstudiantesQuery : IRequest<Result<IReadOnlyList<EstudianteDto>>>;

public sealed class GetEstudiantesQueryHandler : IRequestHandler<GetEstudiantesQuery, Result<IReadOnlyList<EstudianteDto>>>
{
    private readonly IEstudianteRepository _repository;

    public GetEstudiantesQueryHandler(IEstudianteRepository repository) => _repository = repository;

    public async Task<Result<IReadOnlyList<EstudianteDto>>> Handle(GetEstudiantesQuery request, CancellationToken cancellationToken)
    {
        var estudiantes = await _repository.GetAllAsync(cancellationToken);
        var dtos = estudiantes.Select(Map).ToList();
        return Result.Success<IReadOnlyList<EstudianteDto>>(dtos);
    }

    internal static EstudianteDto Map(Estudiante e) =>
        new(e.Id, e.Nombre, e.Email, e.FechaRegistro);
}

public sealed record GetEstudianteByIdQuery(int Id) : IRequest<Result<EstudianteDto>>;

public sealed class GetEstudianteByIdQueryHandler : IRequestHandler<GetEstudianteByIdQuery, Result<EstudianteDto>>
{
    private readonly IEstudianteRepository _repository;

    public GetEstudianteByIdQueryHandler(IEstudianteRepository repository) => _repository = repository;

    public async Task<Result<EstudianteDto>> Handle(GetEstudianteByIdQuery request, CancellationToken cancellationToken)
    {
        var estudiante = await _repository.GetByIdAsync(request.Id, cancellationToken);
        return estudiante is null
            ? Result.Failure<EstudianteDto>("Estudiante no encontrado.")
            : Result.Success(GetEstudiantesQueryHandler.Map(estudiante));
    }
}
