using Auth.Domain.Ports;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Programas.Application.Commands;
using Programas.Application.DTOs;
using Programas.Application.Queries;
using RegistroMs.Api.Security;

namespace RegistroMs.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Estudiante")]
public sealed class ProgramasController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUser _currentUser;

    public ProgramasController(IMediator mediator, ICurrentUser currentUser)
    {
        _mediator = mediator;
        _currentUser = currentUser;
    }

    [HttpGet("activo")]
    public async Task<IActionResult> GetActivo(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetProgramaActivoQuery(), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error);
    }

    [HttpGet("adhesion/{estudianteId:int}")]
    public async Task<IActionResult> GetAdhesion(int estudianteId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetAdhesionEstudianteQuery(estudianteId), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpPost("adherir")]
    public async Task<IActionResult> Adherir([FromBody] AdherirEstudianteRequest request, CancellationToken cancellationToken)
    {
        var denied = EstudianteAuthorization.EnsureOwnEstudiante(_currentUser, request.EstudianteId);
        if (denied is not null) return denied;

        var result = await _mediator.Send(new AdherirEstudianteCommand(request), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }
}
