using Auth.Application.DTOs;
using Auth.Domain.Ports;
using MediatR;
using SharedKernel.Results;

namespace Auth.Application.Queries;

public sealed record GetMeQuery : IRequest<Result<MeResponse>>;

public sealed class GetMeQueryHandler : IRequestHandler<GetMeQuery, Result<MeResponse>>
{
    private readonly ICurrentUser _currentUser;
    private readonly IUsuarioRepository _usuarios;

    public GetMeQueryHandler(ICurrentUser currentUser, IUsuarioRepository usuarios)
    {
        _currentUser = currentUser;
        _usuarios = usuarios;
    }

    public async Task<Result<MeResponse>> Handle(GetMeQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UsuarioId is null)
            return Result.Failure<MeResponse>("No autenticado.");

        var usuario = await _usuarios.GetByIdAsync(_currentUser.UsuarioId.Value, cancellationToken);
        return usuario is null
            ? Result.Failure<MeResponse>("Usuario no encontrado.")
            : Result.Success(new MeResponse(
                usuario.Id,
                usuario.Email,
                usuario.RolCodigo,
                usuario.EstudianteId,
                usuario.EstudianteNombre));
    }
}
