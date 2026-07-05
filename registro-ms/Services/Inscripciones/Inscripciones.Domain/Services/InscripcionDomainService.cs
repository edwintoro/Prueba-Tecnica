using Inscripciones.Domain.Models;
using SharedKernel.Results;

namespace Inscripciones.Domain.Services;

public sealed class InscripcionDomainService
{
    public Result ValidarInscripcion(
        IReadOnlyList<InscripcionDetalleInfo> inscripcionesActuales,
        MateriaCatalogoInfo materiaNueva,
        ProgramaEstudianteInfo programa)
    {
        if (inscripcionesActuales.Count >= programa.MaxMaterias)
            return Result.Failure($"Solo puede seleccionar {programa.MaxMaterias} materias.");

        if (inscripcionesActuales.Any(i => i.MateriaId == materiaNueva.Id))
            return Result.Failure("Ya está inscrito en esta materia.");

        if (inscripcionesActuales.Any(i => i.ProfesorId == materiaNueva.ProfesorId))
            return Result.Failure("No puede tener clases con el mismo profesor.");

        var creditosActuales = inscripcionesActuales.Sum(i => i.Creditos);
        if (creditosActuales + materiaNueva.Creditos > programa.TotalCreditos)
            return Result.Failure($"Supera el límite de {programa.TotalCreditos} créditos del programa.");

        return Result.Success();
    }
}
