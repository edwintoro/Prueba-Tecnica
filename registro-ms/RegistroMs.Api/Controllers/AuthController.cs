using Auth.Application.Commands;
using Auth.Application.DTOs;
using Auth.Application.Queries;
using Auth.Domain.Ports;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace RegistroMs.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUser _currentUser;

    public AuthController(IMediator mediator, ICurrentUser currentUser)
    {
        _mediator = mediator;
        _currentUser = currentUser;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new LoginCommand(request), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : Unauthorized(new { error = result.Error });
    }

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterEstudianteRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new RegisterEstudianteCommand(request), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }

    [AllowAnonymous]
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new RefreshTokenCommand(request), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : Unauthorized(new { error = result.Error });
    }

    [Authorize(Roles = "Estudiante,Administrador")]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        if (_currentUser.SessionId is null)
            return BadRequest(new { error = "Sesión no encontrada." });

        await _mediator.Send(new LogoutCommand(_currentUser.SessionId.Value), cancellationToken);
        return NoContent();
    }

    [Authorize(Roles = "Estudiante,Administrador")]
    [HttpGet("me")]
    public async Task<IActionResult> Me(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetMeQuery(), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : Unauthorized(new { error = result.Error });
    }
}
