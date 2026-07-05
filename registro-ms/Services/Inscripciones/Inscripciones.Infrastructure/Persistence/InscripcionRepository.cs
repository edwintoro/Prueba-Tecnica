using System.Data.Common;
using Inscripciones.Domain.Entities;
using Inscripciones.Domain.Ports;
using Inscripciones.Infrastructure.Persistence.Sql;
using Persistence.Abstractions;

namespace Inscripciones.Infrastructure.Persistence;

/// <summary>
/// Adaptador de persistencia (Hexagonal) usando ADO.NET.
/// </summary>
public sealed class InscripcionRepository : IInscripcionRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly InscripcionSqlDialectFactory _dialectFactory;
    private readonly IEstudiantesClient _estudiantesClient;

    public InscripcionRepository(
        IDbConnectionFactory connectionFactory,
        InscripcionSqlDialectFactory dialectFactory,
        IEstudiantesClient estudiantesClient)
    {
        _connectionFactory = connectionFactory;
        _dialectFactory = dialectFactory;
        _estudiantesClient = estudiantesClient;
    }

    public async Task<IReadOnlyList<Inscripcion>> GetByEstudianteIdAsync(
        int estudianteId,
        CancellationToken cancellationToken = default)
    {
        var sql = _dialectFactory.Create();
        await using var conn = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql.SelectByEstudianteId;
        AddParameter(cmd, "@estudianteId", estudianteId);
        return await ReadAllAsync(cmd, cancellationToken);
    }

    public async Task<Inscripcion?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var sql = _dialectFactory.Create();
        await using var conn = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql.SelectById;
        AddParameter(cmd, "@id", id);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? Map(reader) : null;
    }

    public async Task<bool> ExistsByEstudianteAndMateriaAsync(
        int estudianteId,
        int materiaId,
        CancellationToken cancellationToken = default)
    {
        var sql = _dialectFactory.Create();
        await using var conn = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql.ExistsByEstudianteAndMateria;
        AddParameter(cmd, "@estudianteId", estudianteId);
        AddParameter(cmd, "@materiaId", materiaId);
        var count = Convert.ToInt32(await cmd.ExecuteScalarAsync(cancellationToken));
        return count > 0;
    }

    public async Task<IReadOnlyList<int>> GetEstudianteIdsByMateriaAsync(
        int materiaId,
        int? excludeEstudianteId = null,
        CancellationToken cancellationToken = default)
    {
        var sql = _dialectFactory.Create();
        await using var conn = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = excludeEstudianteId.HasValue
            ? sql.SelectEstudianteIdsByMateriaExcluding
            : sql.SelectEstudianteIdsByMateria;
        AddParameter(cmd, "@materiaId", materiaId);
        if (excludeEstudianteId.HasValue)
            AddParameter(cmd, "@excludeEstudianteId", excludeEstudianteId.Value);

        var ids = new List<int>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
            ids.Add(reader.GetInt32(0));

        return ids;
    }

    public async Task<IReadOnlyList<string>> GetCompanerosNombresAsync(
        int materiaId,
        int excludeEstudianteId,
        CancellationToken cancellationToken = default)
    {
        var estudianteIds = await GetEstudianteIdsByMateriaAsync(materiaId, excludeEstudianteId, cancellationToken);
        if (estudianteIds.Count == 0)
            return Array.Empty<string>();

        var nombres = new List<string>(estudianteIds.Count);

        foreach (var estudianteId in estudianteIds)
        {
            var nombre = await _estudiantesClient.ObtenerNombreAsync(estudianteId, cancellationToken);
            if (!string.IsNullOrWhiteSpace(nombre))
                nombres.Add(nombre);
        }

        return nombres.OrderBy(n => n, StringComparer.OrdinalIgnoreCase).ToList();
    }

    public async Task<int> AddAsync(Inscripcion inscripcion, CancellationToken cancellationToken = default)
    {
        var sql = _dialectFactory.Create();
        await using var conn = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql.Insert;
        AddParameter(cmd, "@estudianteId", inscripcion.EstudianteId);
        AddParameter(cmd, "@materiaId", inscripcion.MateriaId);
        AddParameter(cmd, "@fechaInscripcion", inscripcion.FechaInscripcion);

        if (_connectionFactory.Provider == DatabaseProvider.PostgreSql)
            return Convert.ToInt32(await cmd.ExecuteScalarAsync(cancellationToken));

        await cmd.ExecuteNonQueryAsync(cancellationToken);
        await using var idCmd = conn.CreateCommand();
        idCmd.CommandText = sql.LastInsertId;
        return Convert.ToInt32(await idCmd.ExecuteScalarAsync(cancellationToken));
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var sql = _dialectFactory.Create();
        await using var conn = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql.Delete;
        AddParameter(cmd, "@id", id);
        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task<IReadOnlyList<Inscripcion>> ReadAllAsync(DbCommand cmd, CancellationToken cancellationToken)
    {
        var list = new List<Inscripcion>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
            list.Add(Map(reader));
        return list;
    }

    private static Inscripcion Map(DbDataReader reader) =>
        Inscripcion.Rehydrate(
            reader.GetInt32(reader.GetOrdinal("id")),
            reader.GetInt32(reader.GetOrdinal("estudiante_id")),
            reader.GetInt32(reader.GetOrdinal("materia_id")),
            reader.GetDateTime(reader.GetOrdinal("fecha_inscripcion")));

    private static void AddParameter(DbCommand cmd, string name, object value)
    {
        var param = cmd.CreateParameter();
        param.ParameterName = name;
        param.Value = value;
        cmd.Parameters.Add(param);
    }
}
