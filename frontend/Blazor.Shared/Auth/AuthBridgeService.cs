using System.Text.Json;
using Microsoft.JSInterop;

namespace Blazor.Shared.Auth;

public sealed class AuthBridgeService : IDisposable
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly ITokenStorage _tokens;
    private DotNetObjectReference<AuthBridgeService>? _ref;
    private bool _initialized;

    public AuthBridgeService(ITokenStorage tokens) => _tokens = tokens;

    public bool IsReady { get; private set; }

    public event Action? Ready;

    [JSInvokable]
    public void ReceiveAuthJson(string json) => TrySetSession(json);

    public async Task InitializeAsync(IJSRuntime js)
    {
        if (_initialized) return;
        _initialized = true;

        var stored = await js.InvokeAsync<string?>("authBridge.getStoredSessionJson");
        TrySetSession(stored);

        var shell = await js.InvokeAsync<string?>("authBridge.getShellSessionJson");
        TrySetSession(shell);

        _ref = DotNetObjectReference.Create(this);
        await js.InvokeVoidAsync("authBridge.init", _ref);

        IsReady = _tokens.IsAuthenticated;
        Ready?.Invoke();
    }

    private void TrySetSession(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return;
        try
        {
            var session = JsonSerializer.Deserialize<AuthSession>(json, JsonOptions);
            if (session is not null && !string.IsNullOrWhiteSpace(session.AccessToken))
            {
                _tokens.Set(session);
                if (!IsReady)
                {
                    IsReady = true;
                    Ready?.Invoke();
                }
            }
        }
        catch
        {
            // ignored — next poll or postMessage will retry
        }
    }

    public void Dispose() => _ref?.Dispose();
}
