using Persistence.Abstractions;

namespace Inscripciones.Infrastructure.Persistence.Sql;

public interface IInscripcionSqlDialect
{
    string SelectByEstudianteId { get; }
    string SelectById { get; }
    string ExistsByEstudianteAndMateria { get; }
    string SelectEstudianteIdsByMateria { get; }
    string SelectEstudianteIdsByMateriaExcluding { get; }
    string Insert { get; }
    string Delete { get; }
    string LastInsertId { get; }
}

public sealed class MySqlInscripcionSqlDialect : IInscripcionSqlDialect
{
    public string SelectByEstudianteId =>
        "SELECT id, estudiante_id, materia_id, fecha_inscripcion FROM inscripciones WHERE estudiante_id = @estudianteId ORDER BY fecha_inscripcion";

    public string SelectById =>
        "SELECT id, estudiante_id, materia_id, fecha_inscripcion FROM inscripciones WHERE id = @id";

    public string ExistsByEstudianteAndMateria =>
        "SELECT COUNT(*) FROM inscripciones WHERE estudiante_id = @estudianteId AND materia_id = @materiaId";

    public string SelectEstudianteIdsByMateria =>
        "SELECT estudiante_id FROM inscripciones WHERE materia_id = @materiaId ORDER BY estudiante_id";

    public string SelectEstudianteIdsByMateriaExcluding =>
        "SELECT estudiante_id FROM inscripciones WHERE materia_id = @materiaId AND estudiante_id != @excludeEstudianteId ORDER BY estudiante_id";

    public string Insert =>
        "INSERT INTO inscripciones (estudiante_id, materia_id, fecha_inscripcion) VALUES (@estudianteId, @materiaId, @fechaInscripcion)";

    public string Delete => "DELETE FROM inscripciones WHERE id = @id";

    public string LastInsertId => "SELECT LAST_INSERT_ID()";
}

public sealed class PostgreSqlInscripcionSqlDialect : IInscripcionSqlDialect
{
    public string SelectByEstudianteId =>
        "SELECT id, estudiante_id, materia_id, fecha_inscripcion FROM inscripciones WHERE estudiante_id = @estudianteId ORDER BY fecha_inscripcion";

    public string SelectById =>
        "SELECT id, estudiante_id, materia_id, fecha_inscripcion FROM inscripciones WHERE id = @id";

    public string ExistsByEstudianteAndMateria =>
        "SELECT COUNT(*) FROM inscripciones WHERE estudiante_id = @estudianteId AND materia_id = @materiaId";

    public string SelectEstudianteIdsByMateria =>
        "SELECT estudiante_id FROM inscripciones WHERE materia_id = @materiaId ORDER BY estudiante_id";

    public string SelectEstudianteIdsByMateriaExcluding =>
        "SELECT estudiante_id FROM inscripciones WHERE materia_id = @materiaId AND estudiante_id != @excludeEstudianteId ORDER BY estudiante_id";

    public string Insert =>
        "INSERT INTO inscripciones (estudiante_id, materia_id, fecha_inscripcion) VALUES (@estudianteId, @materiaId, @fechaInscripcion) RETURNING id";

    public string Delete => "DELETE FROM inscripciones WHERE id = @id";

    public string LastInsertId => string.Empty;
}

public sealed class InscripcionSqlDialectFactory
{
    private readonly DatabaseProvider _provider;

    public InscripcionSqlDialectFactory(IDbConnectionFactory connectionFactory)
    {
        _provider = connectionFactory.Provider;
    }

    public IInscripcionSqlDialect Create() => _provider switch
    {
        DatabaseProvider.MySql => new MySqlInscripcionSqlDialect(),
        DatabaseProvider.PostgreSql => new PostgreSqlInscripcionSqlDialect(),
        _ => throw new NotSupportedException($"Provider '{_provider}' not supported.")
    };
}
