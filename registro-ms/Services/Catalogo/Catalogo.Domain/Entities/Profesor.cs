using SharedKernel.Primitives;

namespace Catalogo.Domain.Entities;

public sealed class Profesor : Entity<int>
{
    public string Nombre { get; private set; } = string.Empty;

    private Profesor() { }

    public static Profesor Create(string nombre) =>
        new() { Nombre = nombre.Trim() };

    public static Profesor Rehydrate(int id, string nombre) =>
        new() { Id = id, Nombre = nombre };

    public void Update(string nombre) => Nombre = nombre.Trim();
}
