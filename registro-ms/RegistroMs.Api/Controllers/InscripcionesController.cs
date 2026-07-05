using Auth.Domain.Ports;
using Inscripciones.Application.Commands;
using Inscripciones.Application.DTOs;
using Inscripciones.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RegistroMs.Api.Security;

namespace RegistroMs.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Estudiante")]
public sealed class InscripcionesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUser _currentUser;

    public InscripcionesController(IMediator mediator, ICurrentUser currentUser)
    {
        _mediator = mediator;
        _currentUser = currentUser;
    }

    [HttpGet("estudiantes/{estudianteId:int}")]
    public async Task<IActionResult> GetByEstudiante(int estudianteId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetInscripcionesByEstudianteQuery(estudianteId), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpPost]
    public async Task<IActionResult> Inscribir([FromBody] InscribirMateriaRequest request, CancellationToken cancellationToken)
    {
        var denied = EstudianteAuthorization.EnsureOwnEstudiante(_currentUser, request.EstudianteId);
        if (denied is not null) return denied;

        var result = await _mediator.Send(new InscribirMateriaCommand(request), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Cancelar(int id, CancellationToken cancellationToken)
    {
        if (_currentUser.EstudianteId is null)
            return Unauthorized(new { error = "No autenticado." });

        var inscripciones = await _mediator.Send(
            new GetInscripcionesByEstudianteQuery(_currentUser.EstudianteId.Value),
            cancellationToken);
        if (!inscripciones.IsSuccess || inscripciones.Value!.All(i => i.Id != id))
            return Forbid();

        var result = await _mediator.Send(new CancelarInscripcionCommand(id), cancellationToken);
        return result.IsSuccess ? NoContent() : NotFound(result.Error);
    }

    [HttpGet("materias/{materiaId:int}/companeros")]
    public async Task<IActionResult> GetCompaneros(
        int materiaId,
        [FromQuery] int estudianteId,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetCompanerosQuery(materiaId, estudianteId), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }
}
