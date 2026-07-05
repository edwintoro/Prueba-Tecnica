using System.Net;
using System.Net.Http.Headers;
using Microsoft.JSInterop;

namespace Blazor.Shared.Auth;

public sealed class AuthMessageHandler : DelegatingHandler
{
    private readonly ITokenStorage _tokens;
    private readonly IAuthApiClient _auth;
    private readonly IJSRuntime _js;

    public AuthMessageHandler(ITokenStorage tokens, IAuthApiClient auth, IJSRuntime js)
    {
        _tokens = tokens;
        _auth = auth;
        _js = js;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        ApplyToken(request);

        var response = await base.SendAsync(request, cancellationToken);
        if (response.StatusCode != HttpStatusCode.Unauthorized)
            return response;

        if (_tokens.Current is null)
        {
            try { await _js.InvokeVoidAsync("authBridge.clearStoredSession"); } catch { }
            try { await _js.InvokeVoidAsync("authBridge.notifyLogout"); } catch { }
            return response;
        }

        var refreshed = await _auth.RefreshAsync(new RefreshRequest(
            _tokens.Current.RefreshToken,
            _tokens.Current.SessionId));

        if (refreshed is not null)
        {
            _tokens.Set(refreshed);
            try { await _js.InvokeVoidAsync("authBridge.notifyLogin", refreshed); } catch { }
            response.Dispose();
            ApplyToken(request);
            return await base.SendAsync(request, cancellationToken);
        }

        _tokens.Clear();
        try { await _js.InvokeVoidAsync("authBridge.clearStoredSession"); } catch { }
        try { await _js.InvokeVoidAsync("authBridge.notifyLogout"); } catch { }
        return response;
    }

    private void ApplyToken(HttpRequestMessage request)
    {
        if (_tokens.Current?.AccessToken is { } token)
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }
}
