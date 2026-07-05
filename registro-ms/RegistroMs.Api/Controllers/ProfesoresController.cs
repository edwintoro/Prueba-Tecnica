using Catalogo.Application.Commands;
using Catalogo.Application.DTOs;
using Catalogo.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace RegistroMs.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class ProfesoresController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProfesoresController(IMediator mediator) => _mediator = mediator;

    [Authorize(Roles = "Estudiante,Administrador")]
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetProfesoresQuery(), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [Authorize(Roles = "Administrador")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProfesorRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new CreateProfesorCommand(request), cancellationToken);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetAll), result.Value)
            : BadRequest(result.Error);
    }

    [Authorize(Roles = "Administrador")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateProfesorRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new UpdateProfesorCommand(id, request), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [Authorize(Roles = "Administrador")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new DeleteProfesorCommand(id), cancellationToken);
        return result.IsSuccess ? NoContent() : BadRequest(result.Error);
    }
}
