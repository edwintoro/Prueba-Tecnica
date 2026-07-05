using System.Text.Json;
using Blazor.Shared.Auth;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Orquestador.Shell.Services;

public sealed class ShellAuthService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly IJSRuntime _js;
    private AuthSession? _session;

    public ShellAuthService(IJSRuntime js) => _js = js;

    public AuthSession? Session => _session;

    public async Task EnsureRelayAsync()
    {
        await _js.InvokeVoidAsync("shellAuth.initRelay");
    }

    public async Task<bool> IsAuthenticatedAsync()
    {
        _session = await _js.InvokeAsync<AuthSession?>("shellAuth.get");
        return _session is not null && !string.IsNullOrWhiteSpace(_session.AccessToken);
    }

    public async Task<string> BuildMfUrlAsync(string mfBaseUrl, string path)
    {
        _session ??= await _js.InvokeAsync<AuthSession?>("shellAuth.get");
        var url = $"{mfBaseUrl.TrimEnd('/')}/{path.TrimStart('/')}";
        if (_session is null || string.IsNullOrWhiteSpace(_session.AccessToken))
            return url;

        var json = JsonSerializer.Serialize(_session, JsonOptions);
        return $"{url}#auth={Uri.EscapeDataString(json)}";
    }

    public async Task RelayAuthToFrameAsync(ElementReference frame)
    {
        await _js.InvokeVoidAsync("shellAuth.relayToFrame", frame);
    }

    public async Task SetSessionAsync(AuthSession session)
    {
        _session = session;
        await _js.InvokeVoidAsync("shellAuth.set", session);
    }

    public void SetSession(AuthSession session) => _session = session;

    public async Task ClearAsync()
    {
        _session = null;
        await _js.InvokeVoidAsync("shellAuth.clear");
    }

    public bool IsAdministrador() => AppRoles.IsAdministrador(Session?.Rol);
    public bool IsEstudiante() => AppRoles.IsEstudiante(Session?.Rol);
    public string? Rol => Session?.Rol;
}
