namespace Inscripciones.Infrastructure.Options;

public sealed class ServicesOptions
{
    public const string SectionName = "Services";

    public string Programas { get; set; } = string.Empty;
    public string Catalogo { get; set; } = string.Empty;
    public string Estudiantes { get; set; } = string.Empty;
}
