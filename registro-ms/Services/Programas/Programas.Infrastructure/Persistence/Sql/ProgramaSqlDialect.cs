using Persistence.Abstractions;

namespace Programas.Infrastructure.Persistence.Sql;

/// <summary>
/// Strategy Pattern: dialectos SQL por proveedor de BD.
/// </summary>
public interface IProgramaSqlDialect
{
    string SelectActivo { get; }
    string SelectAdhesionByEstudianteId { get; }
    string ExistsAdhesionByEstudianteId { get; }
    string InsertAdhesion { get; }
    string LastInsertId { get; }
}

public sealed class MySqlProgramaSqlDialect : IProgramaSqlDialect
{
    public string SelectActivo =>
        "SELECT id, nombre, creditos_por_materia, max_materias, total_creditos, activo FROM programas_creditos WHERE activo = 1 LIMIT 1";

    public string SelectAdhesionByEstudianteId =>
        "SELECT id, estudiante_id, programa_id, fecha_adhesion FROM estudiante_programa WHERE estudiante_id = @estudianteId";

    public string ExistsAdhesionByEstudianteId =>
        "SELECT COUNT(*) FROM estudiante_programa WHERE estudiante_id = @estudianteId";

    public string InsertAdhesion =>
        "INSERT INTO estudiante_programa (estudiante_id, programa_id, fecha_adhesion) VALUES (@estudianteId, @programaId, @fechaAdhesion)";

    public string LastInsertId => "SELECT LAST_INSERT_ID()";
}

public sealed class PostgreSqlProgramaSqlDialect : IProgramaSqlDialect
{
    public string SelectActivo =>
        "SELECT id, nombre, creditos_por_materia, max_materias, total_creditos, activo FROM programas_creditos WHERE activo = TRUE LIMIT 1";

    public string SelectAdhesionByEstudianteId =>
        "SELECT id, estudiante_id, programa_id, fecha_adhesion FROM estudiante_programa WHERE estudiante_id = @estudianteId";

    public string ExistsAdhesionByEstudianteId =>
        "SELECT COUNT(*) FROM estudiante_programa WHERE estudiante_id = @estudianteId";

    public string InsertAdhesion =>
        "INSERT INTO estudiante_programa (estudiante_id, programa_id, fecha_adhesion) VALUES (@estudianteId, @programaId, @fechaAdhesion) RETURNING id";

    public string LastInsertId => string.Empty;
}

public sealed class ProgramaSqlDialectFactory
{
    private readonly DatabaseProvider _provider;

    public ProgramaSqlDialectFactory(IDbConnectionFactory connectionFactory)
    {
        _provider = connectionFactory.Provider;
    }

    public IProgramaSqlDialect Create() => _provider switch
    {
        DatabaseProvider.MySql => new MySqlProgramaSqlDialect(),
        DatabaseProvider.PostgreSql => new PostgreSqlProgramaSqlDialect(),
        _ => throw new NotSupportedException($"Provider '{_provider}' not supported.")
    };
}
