using Catalogo.Application.DTOs;
using Catalogo.Application.Queries;
using Catalogo.Domain.Entities;
using Catalogo.Domain.Ports;
using MediatR;
using Microsoft.Extensions.Caching.Memory;
using SharedKernel.Results;

namespace Catalogo.Application.Commands;

public sealed record CreateMateriaCommand(CreateMateriaRequest Request) : IRequest<Result<MateriaDto>>;
public sealed record UpdateMateriaCommand(int Id, UpdateMateriaRequest Request) : IRequest<Result<MateriaDto>>;
public sealed record DeleteMateriaCommand(int Id) : IRequest<Result>;

public sealed class CreateMateriaCommandHandler : IRequestHandler<CreateMateriaCommand, Result<MateriaDto>>
{
    private readonly IMateriaRepository _materias;
    private readonly IProfesorRepository _profesores;
    private readonly IMemoryCache _cache;

    public CreateMateriaCommandHandler(IMateriaRepository materias, IProfesorRepository profesores, IMemoryCache cache)
    {
        _materias = materias;
        _profesores = profesores;
        _cache = cache;
    }

    public async Task<Result<MateriaDto>> Handle(CreateMateriaCommand command, CancellationToken cancellationToken)
    {
        var req = command.Request;
        if (string.IsNullOrWhiteSpace(req.Nombre))
            return Result.Failure<MateriaDto>("El nombre es obligatorio.");
        if (req.Creditos <= 0)
            return Result.Failure<MateriaDto>("Los créditos deben ser mayores a cero.");

        var totalMaterias = await _materias.CountAllAsync(cancellationToken);
        if (totalMaterias >= Materia.MaxMateriasEnCatalogo)
            return Result.Failure<MateriaDto>($"Solo se pueden registrar {Materia.MaxMateriasEnCatalogo} materias en el catálogo.");

        var profesor = await _profesores.GetByIdAsync(req.ProfesorId, cancellationToken);
        if (profesor is null)
            return Result.Failure<MateriaDto>("Profesor no encontrado.");

        var count = await _materias.CountByProfesorAsync(req.ProfesorId, cancellationToken: cancellationToken);
        if (count >= Materia.MaxMateriasPorProfesor)
            return Result.Failure<MateriaDto>($"Un profesor solo puede dictar {Materia.MaxMateriasPorProfesor} materias.");

        var programaId = req.ProgramaId ?? await _materias.GetActiveProgramaIdAsync(cancellationToken);
        if (programaId is null)
            return Result.Failure<MateriaDto>("No hay un programa de créditos activo.");

        var materia = Materia.Create(req.Nombre, req.Creditos, req.ProfesorId, programaId.Value);
        var id = await _materias.AddAsync(materia, cancellationToken);
        CatalogoCache.Invalidate(_cache);

        var created = await _materias.GetByIdWithProfesorAsync(id, cancellationToken);
        return created is null
            ? Result.Failure<MateriaDto>("No se pudo crear la materia.")
            : Result.Success(GetMateriasQueryHandler.Map(created));
    }
}

public sealed class UpdateMateriaCommandHandler : IRequestHandler<UpdateMateriaCommand, Result<MateriaDto>>
{
    private readonly IMateriaRepository _materias;
    private readonly IProfesorRepository _profesores;
    private readonly IMemoryCache _cache;

    public UpdateMateriaCommandHandler(IMateriaRepository materias, IProfesorRepository profesores, IMemoryCache cache)
    {
        _materias = materias;
        _profesores = profesores;
        _cache = cache;
    }

    public async Task<Result<MateriaDto>> Handle(UpdateMateriaCommand command, CancellationToken cancellationToken)
    {
        var existing = await _materias.GetByIdWithProfesorAsync(command.Id, cancellationToken);
        if (existing is null)
            return Result.Failure<MateriaDto>("Materia no encontrada.");

        var req = command.Request;
        if (string.IsNullOrWhiteSpace(req.Nombre))
            return Result.Failure<MateriaDto>("El nombre es obligatorio.");
        if (req.Creditos <= 0)
            return Result.Failure<MateriaDto>("Los créditos deben ser mayores a cero.");

        var profesor = await _profesores.GetByIdAsync(req.ProfesorId, cancellationToken);
        if (profesor is null)
            return Result.Failure<MateriaDto>("Profesor no encontrado.");

        var count = await _materias.CountByProfesorAsync(req.ProfesorId, command.Id, cancellationToken);
        if (count >= Materia.MaxMateriasPorProfesor)
            return Result.Failure<MateriaDto>($"Un profesor solo puede dictar {Materia.MaxMateriasPorProfesor} materias.");

        var programaId = req.ProgramaId ?? existing.ProgramaId;
        var materia = Materia.Rehydrate(command.Id, req.Nombre, req.Creditos, req.ProfesorId, programaId);
        await _materias.UpdateAsync(materia, cancellationToken);
        CatalogoCache.Invalidate(_cache);

        var updated = await _materias.GetByIdWithProfesorAsync(command.Id, cancellationToken);
        return updated is null
            ? Result.Failure<MateriaDto>("No se pudo actualizar la materia.")
            : Result.Success(GetMateriasQueryHandler.Map(updated));
    }
}

public sealed class DeleteMateriaCommandHandler : IRequestHandler<DeleteMateriaCommand, Result>
{
    private readonly IMateriaRepository _materias;
    private readonly IMemoryCache _cache;

    public DeleteMateriaCommandHandler(IMateriaRepository materias, IMemoryCache cache)
    {
        _materias = materias;
        _cache = cache;
    }

    public async Task<Result> Handle(DeleteMateriaCommand command, CancellationToken cancellationToken)
    {
        var existing = await _materias.GetByIdWithProfesorAsync(command.Id, cancellationToken);
        if (existing is null)
            return Result.Failure("Materia no encontrada.");

        await _materias.DeleteAsync(command.Id, cancellationToken);
        CatalogoCache.Invalidate(_cache);
        return Result.Success();
    }
}

public sealed record CreateProfesorCommand(CreateProfesorRequest Request) : IRequest<Result<ProfesorDto>>;
public sealed record UpdateProfesorCommand(int Id, UpdateProfesorRequest Request) : IRequest<Result<ProfesorDto>>;
public sealed record DeleteProfesorCommand(int Id) : IRequest<Result>;

public sealed class CreateProfesorCommandHandler : IRequestHandler<CreateProfesorCommand, Result<ProfesorDto>>
{
    private readonly IProfesorRepository _profesores;
    private readonly IMemoryCache _cache;

    public CreateProfesorCommandHandler(IProfesorRepository profesores, IMemoryCache cache)
    {
        _profesores = profesores;
        _cache = cache;
    }

    public async Task<Result<ProfesorDto>> Handle(CreateProfesorCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.Request.Nombre))
            return Result.Failure<ProfesorDto>("El nombre es obligatorio.");

        var profesor = Profesor.Create(command.Request.Nombre);
        var id = await _profesores.AddAsync(profesor, cancellationToken);
        CatalogoCache.Invalidate(_cache);
        return Result.Success(new ProfesorDto(id, profesor.Nombre));
    }
}

public sealed class UpdateProfesorCommandHandler : IRequestHandler<UpdateProfesorCommand, Result<ProfesorDto>>
{
    private readonly IProfesorRepository _profesores;
    private readonly IMemoryCache _cache;

    public UpdateProfesorCommandHandler(IProfesorRepository profesores, IMemoryCache cache)
    {
        _profesores = profesores;
        _cache = cache;
    }

    public async Task<Result<ProfesorDto>> Handle(UpdateProfesorCommand command, CancellationToken cancellationToken)
    {
        var profesor = await _profesores.GetByIdAsync(command.Id, cancellationToken);
        if (profesor is null)
            return Result.Failure<ProfesorDto>("Profesor no encontrado.");
        if (string.IsNullOrWhiteSpace(command.Request.Nombre))
            return Result.Failure<ProfesorDto>("El nombre es obligatorio.");

        profesor.Update(command.Request.Nombre);
        await _profesores.UpdateAsync(profesor, cancellationToken);
        CatalogoCache.Invalidate(_cache);
        return Result.Success(new ProfesorDto(profesor.Id, profesor.Nombre));
    }
}

public sealed class DeleteProfesorCommandHandler : IRequestHandler<DeleteProfesorCommand, Result>
{
    private readonly IProfesorRepository _profesores;
    private readonly IMemoryCache _cache;

    public DeleteProfesorCommandHandler(IProfesorRepository profesores, IMemoryCache cache)
    {
        _profesores = profesores;
        _cache = cache;
    }

    public async Task<Result> Handle(DeleteProfesorCommand command, CancellationToken cancellationToken)
    {
        var profesor = await _profesores.GetByIdAsync(command.Id, cancellationToken);
        if (profesor is null)
            return Result.Failure("Profesor no encontrado.");
        if (await _profesores.HasMateriasAsync(command.Id, cancellationToken))
            return Result.Failure("No se puede eliminar un profesor con materias asignadas.");

        await _profesores.DeleteAsync(command.Id, cancellationToken);
        CatalogoCache.Invalidate(_cache);
        return Result.Success();
    }
}
