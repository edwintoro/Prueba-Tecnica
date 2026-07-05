using Persistence.Abstractions;

namespace Catalogo.Infrastructure.Persistence.Sql;

public interface IMateriaSqlDialect
{
    string SelectAllWithProfesor { get; }
    string SelectByIdWithProfesor { get; }
    string CountByProfesor { get; }
    string CountByProfesorExcluding { get; }
    string Insert { get; }
    string Update { get; }
    string Delete { get; }
    string LastInsertId { get; }
}

public sealed class MySqlMateriaSqlDialect : IMateriaSqlDialect
{
    public string SelectAllWithProfesor =>
        """
        SELECT m.id, m.nombre, m.creditos, m.profesor_id, p.nombre AS profesor_nombre, m.programa_id
        FROM materias m
        INNER JOIN profesores p ON m.profesor_id = p.id
        ORDER BY m.nombre
        """;

    public string SelectByIdWithProfesor =>
        """
        SELECT m.id, m.nombre, m.creditos, m.profesor_id, p.nombre AS profesor_nombre, m.programa_id
        FROM materias m
        INNER JOIN profesores p ON m.profesor_id = p.id
        WHERE m.id = @id
        """;

    public string CountByProfesor =>
        "SELECT COUNT(1) FROM materias WHERE profesor_id = @profesorId";

    public string CountByProfesorExcluding =>
        "SELECT COUNT(1) FROM materias WHERE profesor_id = @profesorId AND id <> @excludeId";

    public string Insert =>
        """
        INSERT INTO materias (nombre, creditos, profesor_id, programa_id)
        VALUES (@nombre, @creditos, @profesorId, @programaId);
        SELECT LAST_INSERT_ID();
        """;

    public string Update =>
        """
        UPDATE materias
        SET nombre = @nombre, creditos = @creditos, profesor_id = @profesorId, programa_id = @programaId
        WHERE id = @id
        """;

    public string Delete => "DELETE FROM materias WHERE id = @id";
    public string LastInsertId => "SELECT LAST_INSERT_ID()";
}

public sealed class PostgreSqlMateriaSqlDialect : IMateriaSqlDialect
{
    public string SelectAllWithProfesor =>
        """
        SELECT m.id, m.nombre, m.creditos, m.profesor_id, p.nombre AS profesor_nombre, m.programa_id
        FROM materias m
        INNER JOIN profesores p ON m.profesor_id = p.id
        ORDER BY m.nombre
        """;

    public string SelectByIdWithProfesor =>
        """
        SELECT m.id, m.nombre, m.creditos, m.profesor_id, p.nombre AS profesor_nombre, m.programa_id
        FROM materias m
        INNER JOIN profesores p ON m.profesor_id = p.id
        WHERE m.id = @id
        """;

    public string CountByProfesor =>
        "SELECT COUNT(1) FROM materias WHERE profesor_id = @profesorId";

    public string CountByProfesorExcluding =>
        "SELECT COUNT(1) FROM materias WHERE profesor_id = @profesorId AND id <> @excludeId";

    public string Insert =>
        """
        INSERT INTO materias (nombre, creditos, profesor_id, programa_id)
        VALUES (@nombre, @creditos, @profesorId, @programaId)
        RETURNING id
        """;

    public string Update =>
        """
        UPDATE materias
        SET nombre = @nombre, creditos = @creditos, profesor_id = @profesorId, programa_id = @programaId
        WHERE id = @id
        """;

    public string Delete => "DELETE FROM materias WHERE id = @id";
    public string LastInsertId => "SELECT 0";
}

public sealed class MateriaSqlDialectFactory
{
    private readonly DatabaseProvider _provider;

    public MateriaSqlDialectFactory(IDbConnectionFactory connectionFactory) =>
        _provider = connectionFactory.Provider;

    public IMateriaSqlDialect Create() => _provider switch
    {
        DatabaseProvider.MySql => new MySqlMateriaSqlDialect(),
        DatabaseProvider.PostgreSql => new PostgreSqlMateriaSqlDialect(),
        _ => throw new NotSupportedException($"Provider '{_provider}' not supported.")
    };
}
