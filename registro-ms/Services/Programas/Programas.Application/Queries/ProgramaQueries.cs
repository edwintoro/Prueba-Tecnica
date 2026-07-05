using Programas.Application.DTOs;
using Programas.Domain.Entities;
using Programas.Domain.Ports;
using MediatR;
using Microsoft.Extensions.Caching.Memory;
using SharedKernel.Results;

namespace Programas.Application.Queries;

public sealed record GetProgramaActivoQuery : IRequest<Result<ProgramaDto>>;

public sealed class GetProgramaActivoQueryHandler : IRequestHandler<GetProgramaActivoQuery, Result<ProgramaDto>>
{
    private const string CacheKey = "programas:activo";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

    private readonly IProgramaRepository _repository;
    private readonly IMemoryCache _cache;

    public GetProgramaActivoQueryHandler(IProgramaRepository repository, IMemoryCache cache)
    {
        _repository = repository;
        _cache = cache;
    }

    public async Task<Result<ProgramaDto>> Handle(GetProgramaActivoQuery request, CancellationToken cancellationToken)
    {
        if (_cache.TryGetValue(CacheKey, out ProgramaDto? cached) && cached is not null)
            return Result.Success(cached);

        var programa = await _repository.GetActivoAsync(cancellationToken);
        if (programa is null)
            return Result.Failure<ProgramaDto>("No hay un programa de créditos activo.");

        var dto = MapPrograma(programa);
        _cache.Set(CacheKey, dto, CacheDuration);
        return Result.Success(dto);
    }

    internal static ProgramaDto MapPrograma(ProgramaCredito p) =>
        new(p.Id, p.Nombre, p.CreditosPorMateria, p.MaxMaterias, p.TotalCreditos, p.Activo);
}

public sealed record GetAdhesionEstudianteQuery(int EstudianteId) : IRequest<Result<AdhesionEstudianteDto>>;

public sealed class GetAdhesionEstudianteQueryHandler : IRequestHandler<GetAdhesionEstudianteQuery, Result<AdhesionEstudianteDto>>
{
    private readonly IProgramaRepository _repository;

    public GetAdhesionEstudianteQueryHandler(IProgramaRepository repository) => _repository = repository;

    public async Task<Result<AdhesionEstudianteDto>> Handle(GetAdhesionEstudianteQuery request, CancellationToken cancellationToken)
    {
        var adhesion = await _repository.GetAdhesionByEstudianteIdAsync(request.EstudianteId, cancellationToken);
        if (adhesion is null)
            return Result.Success(new AdhesionEstudianteDto(false, null, null));

        return Result.Success(new AdhesionEstudianteDto(true, adhesion.ProgramaId, adhesion.FechaAdhesion));
    }
}
