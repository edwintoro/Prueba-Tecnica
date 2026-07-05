using Catalogo.Application.Commands;
using Catalogo.Application.DTOs;
using Catalogo.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace RegistroMs.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class MateriasController : ControllerBase
{
    private readonly IMediator _mediator;

    public MateriasController(IMediator mediator) => _mediator = mediator;

    [Authorize(Roles = "Estudiante,Administrador")]
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetMateriasQuery(), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [Authorize(Roles = "Estudiante,Administrador")]
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetMateriaByIdQuery(id), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error);
    }

    [Authorize(Roles = "Administrador")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateMateriaRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new CreateMateriaCommand(request), cancellationToken);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value)
            : BadRequest(result.Error);
    }

    [Authorize(Roles = "Administrador")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateMateriaRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new UpdateMateriaCommand(id, request), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [Authorize(Roles = "Administrador")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new DeleteMateriaCommand(id), cancellationToken);
        return result.IsSuccess ? NoContent() : BadRequest(result.Error);
    }
}
