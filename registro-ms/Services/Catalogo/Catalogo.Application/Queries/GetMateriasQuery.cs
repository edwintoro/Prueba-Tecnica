using Catalogo.Application.DTOs;
using Catalogo.Domain.Ports;
using Catalogo.Domain.ReadModels;
using MediatR;
using Microsoft.Extensions.Caching.Memory;
using SharedKernel.Results;

namespace Catalogo.Application.Queries;

public sealed record GetMateriasQuery : IRequest<Result<IReadOnlyList<MateriaDto>>>;

public sealed class GetMateriasQueryHandler : IRequestHandler<GetMateriasQuery, Result<IReadOnlyList<MateriaDto>>>
{
    private const string CacheKey = CatalogoCache.MateriasKey;
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

    private readonly IMateriaRepository _repository;
    private readonly IMemoryCache _cache;

    public GetMateriasQueryHandler(IMateriaRepository repository, IMemoryCache cache)
    {
        _repository = repository;
        _cache = cache;
    }

    public async Task<Result<IReadOnlyList<MateriaDto>>> Handle(GetMateriasQuery request, CancellationToken cancellationToken)
    {
        if (_cache.TryGetValue(CacheKey, out IReadOnlyList<MateriaDto>? cached) && cached is not null)
            return Result.Success(cached);

        var materias = await _repository.GetAllWithProfesorAsync(cancellationToken);
        var dtos = materias.Select(Map).ToList();
        _cache.Set(CacheKey, dtos, CacheDuration);
        return Result.Success<IReadOnlyList<MateriaDto>>(dtos);
    }

    internal static MateriaDto Map(MateriaConProfesor m) =>
        new(m.Id, m.Nombre, m.Creditos, m.ProfesorId, m.ProfesorNombre, m.ProgramaId);
}

public sealed record GetMateriaByIdQuery(int Id) : IRequest<Result<MateriaDto>>;

public sealed class GetMateriaByIdQueryHandler : IRequestHandler<GetMateriaByIdQuery, Result<MateriaDto>>
{
    private readonly IMateriaRepository _repository;

    public GetMateriaByIdQueryHandler(IMateriaRepository repository) => _repository = repository;

    public async Task<Result<MateriaDto>> Handle(GetMateriaByIdQuery request, CancellationToken cancellationToken)
    {
        var materia = await _repository.GetByIdWithProfesorAsync(request.Id, cancellationToken);
        return materia is null
            ? Result.Failure<MateriaDto>("Materia no encontrada.")
            : Result.Success(GetMateriasQueryHandler.Map(materia));
    }
}
