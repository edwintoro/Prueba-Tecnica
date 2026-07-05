using Catalogo.Application.DTOs;
using Catalogo.Domain.Entities;
using Catalogo.Domain.Ports;
using MediatR;
using SharedKernel.Results;

namespace Catalogo.Application.Queries;

public sealed record GetProfesoresQuery : IRequest<Result<IReadOnlyList<ProfesorDto>>>;

public sealed class GetProfesoresQueryHandler : IRequestHandler<GetProfesoresQuery, Result<IReadOnlyList<ProfesorDto>>>
{
    private readonly IProfesorRepository _repository;

    public GetProfesoresQueryHandler(IProfesorRepository repository) => _repository = repository;

    public async Task<Result<IReadOnlyList<ProfesorDto>>> Handle(GetProfesoresQuery request, CancellationToken cancellationToken)
    {
        var profesores = await _repository.GetAllAsync(cancellationToken);
        var dtos = profesores.Select(Map).ToList();
        return Result.Success<IReadOnlyList<ProfesorDto>>(dtos);
    }

    internal static ProfesorDto Map(Profesor p) => new(p.Id, p.Nombre);
}
