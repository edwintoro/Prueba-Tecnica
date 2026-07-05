using SharedKernel.Guards;
using SharedKernel.Primitives;

namespace Estudiantes.Domain.Entities;

public sealed class Estudiante : Entity<int>
{
    public string Nombre { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public DateTime FechaRegistro { get; private set; }

    private Estudiante() { }

    public static Estudiante Create(string nombre, string email)
    {
        return new Estudiante
        {
            Nombre = Guard.AgainstNullOrWhiteSpace(nombre, nameof(nombre)),
            Email = Guard.AgainstNullOrWhiteSpace(email, nameof(email)).ToLowerInvariant(),
            FechaRegistro = DateTime.UtcNow
        };
    }

    public void Update(string nombre, string email)
    {
        Nombre = Guard.AgainstNullOrWhiteSpace(nombre, nameof(nombre));
        Email = Guard.AgainstNullOrWhiteSpace(email, nameof(email)).ToLowerInvariant();
    }

    public static Estudiante Rehydrate(int id, string nombre, string email, DateTime fechaRegistro) =>
        new() { Id = id, Nombre = nombre, Email = email, FechaRegistro = fechaRegistro };
}
