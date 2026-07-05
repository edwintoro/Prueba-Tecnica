using Inscripciones.Domain.Ports;
using MediatR;
using SharedKernel.Results;

namespace Inscripciones.Application.Queries;

public sealed record GetCompanerosQuery(int MateriaId, int EstudianteId)
    : IRequest<Result<IReadOnlyList<string>>>;

public sealed class GetCompanerosQueryHandler : IRequestHandler<GetCompanerosQuery, Result<IReadOnlyList<string>>>
{
    private readonly IInscripcionRepository _repository;
    private readonly ICatalogoClient _catalogoClient;

    public GetCompanerosQueryHandler(IInscripcionRepository repository, ICatalogoClient catalogoClient)
    {
        _repository = repository;
        _catalogoClient = catalogoClient;
    }

    public async Task<Result<IReadOnlyList<string>>> Handle(GetCompanerosQuery request, CancellationToken cancellationToken)
    {
        var materia = await _catalogoClient.ObtenerMateriaAsync(request.MateriaId, cancellationToken);
        if (materia is null)
            return Result.Failure<IReadOnlyList<string>>("La materia seleccionada no existe.");

        var nombres = await _repository.GetCompanerosNombresAsync(
            request.MateriaId,
            request.EstudianteId,
            cancellationToken);

        return Result.Success(nombres);
    }
}
