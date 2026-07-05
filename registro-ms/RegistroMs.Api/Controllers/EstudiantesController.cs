using Estudiantes.Application.Commands;
using Estudiantes.Application.DTOs;
using Estudiantes.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace RegistroMs.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class EstudiantesController : ControllerBase
{
    private readonly IMediator _mediator;

    public EstudiantesController(IMediator mediator) => _mediator = mediator;

    [Authorize(Roles = "Estudiante,Administrador")]
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetEstudiantesQuery(), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [Authorize(Roles = "Estudiante,Administrador")]
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetEstudianteByIdQuery(id), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error);
    }

    [Authorize(Roles = "Administrador")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateEstudianteRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new CreateEstudianteCommand(request), cancellationToken);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value)
            : BadRequest(result.Error);
    }

    [Authorize(Roles = "Administrador")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateEstudianteRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new UpdateEstudianteCommand(id, request), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [Authorize(Roles = "Administrador")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new DeleteEstudianteCommand(id), cancellationToken);
        return result.IsSuccess ? NoContent() : NotFound(result.Error);
    }
}
