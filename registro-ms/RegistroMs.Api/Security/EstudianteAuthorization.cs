using Auth.Domain.Ports;
using Microsoft.AspNetCore.Mvc;

namespace RegistroMs.Api.Security;

public static class EstudianteAuthorization
{
    public static IActionResult? EnsureOwnEstudiante(ICurrentUser user, int estudianteId)
    {
        if (!user.IsAuthenticated || user.EstudianteId is null)
            return new UnauthorizedObjectResult(new { error = "No autenticado." });

        if (user.EstudianteId.Value != estudianteId)
            return new ObjectResult(new { error = "No puede operar sobre otro estudiante." }) { StatusCode = 403 };

        return null;
    }
}
