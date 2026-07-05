using Microsoft.Extensions.Caching.Memory;

namespace Catalogo.Application;

internal static class CatalogoCache
{
    internal const string MateriasKey = "catalogo:materias";
    internal const string ProfesoresKey = "catalogo:profesores";

    internal static void Invalidate(IMemoryCache cache)
    {
        cache.Remove(MateriasKey);
        cache.Remove(ProfesoresKey);
    }
}
