using SharedKernel.Guards;
using SharedKernel.Primitives;

namespace Programas.Domain.Entities;

public sealed class ProgramaCredito : Entity<int>
{
    public string Nombre { get; private set; } = string.Empty;
    public int CreditosPorMateria { get; private set; } = 3;
    public int MaxMaterias { get; private set; } = 3;
    public int TotalCreditos { get; private set; } = 9;
    public bool Activo { get; private set; }

    private ProgramaCredito() { }

    public static ProgramaCredito Create(string nombre, int creditosPorMateria = 3, int maxMaterias = 3, int totalCreditos = 9, bool activo = true)
    {
        return new ProgramaCredito
        {
            Nombre = Guard.AgainstNullOrWhiteSpace(nombre, nameof(nombre)),
            CreditosPorMateria = creditosPorMateria,
            MaxMaterias = maxMaterias,
            TotalCreditos = totalCreditos,
            Activo = activo
        };
    }

    public static ProgramaCredito Rehydrate(
        int id,
        string nombre,
        int creditosPorMateria,
        int maxMaterias,
        int totalCreditos,
        bool activo) =>
        new()
        {
            Id = id,
            Nombre = nombre,
            CreditosPorMateria = creditosPorMateria,
            MaxMaterias = maxMaterias,
            TotalCreditos = totalCreditos,
            Activo = activo
        };
}
