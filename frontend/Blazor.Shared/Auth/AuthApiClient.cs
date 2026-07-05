using System.Net.Http.Headers;
using System.Net.Http.Json;
using Blazor.Shared.Options;
using Microsoft.Extensions.Options;

namespace Blazor.Shared.Auth;

public sealed record AuthOperationResult<T>(T? Value, string? Error)
{
    public bool IsSuccess => Value is not null;
}

public interface IAuthApiClient
{
    Task<AuthOperationResult<AuthSession>> LoginAsync(LoginRequest request);
    Task<AuthOperationResult<AuthSession>> RegisterAsync(RegisterRequest request);
    Task<AuthSession?> RefreshAsync(RefreshRequest request);
    Task<MeResponse?> GetMeAsync(string accessToken);
    Task LogoutAsync(string accessToken);
}

public sealed class AuthApiClient : IAuthApiClient
{
    private readonly HttpClient _http;

    public AuthApiClient(HttpClient http, IOptions<ApiGatewayOptions> options)
    {
        _http = http;
        _http.BaseAddress = new Uri(options.Value.BaseUrl);
    }

    public async Task<AuthOperationResult<AuthSession>> LoginAsync(LoginRequest request)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("api/auth/login", request);
            return await ReadSessionAsync(response, "Credenciales inválidas.");
        }
        catch (Exception ex) when (IsConnectionError(ex))
        {
            return new AuthOperationResult<AuthSession>(null, ConnectionErrorMessage);
        }
        catch (Exception)
        {
            return new AuthOperationResult<AuthSession>(null, "Ocurrió un error inesperado. Intente de nuevo.");
        }
    }

    public async Task<AuthOperationResult<AuthSession>> RegisterAsync(RegisterRequest request)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("api/auth/register", request);
            return await ReadSessionAsync(response, "No se pudo completar el registro.");
        }
        catch (Exception ex) when (IsConnectionError(ex))
        {
            return new AuthOperationResult<AuthSession>(null, ConnectionErrorMessage);
        }
        catch (Exception)
        {
            return new AuthOperationResult<AuthSession>(null, "Ocurrió un error inesperado. Intente de nuevo.");
        }
    }

    public async Task<AuthSession?> RefreshAsync(RefreshRequest request)
    {
        var response = await _http.PostAsJsonAsync("api/auth/refresh", request);
        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<AuthSession>()
            : null;
    }

    public async Task<MeResponse?> GetMeAsync(string accessToken)
    {
        using var req = new HttpRequestMessage(HttpMethod.Get, "api/auth/me");
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var response = await _http.SendAsync(req);
        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<MeResponse>()
            : null;
    }

    public async Task LogoutAsync(string accessToken)
    {
        using var req = new HttpRequestMessage(HttpMethod.Post, "api/auth/logout");
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        await _http.SendAsync(req);
    }

    private const string ConnectionErrorMessage =
        "No se pudo conectar con el servidor. Verifique que el sistema esté en ejecución e intente de nuevo.";

    private static bool IsConnectionError(Exception ex) =>
        ex is HttpRequestException or TaskCanceledException
        || ex.Message.Contains("Failed to fetch", StringComparison.OrdinalIgnoreCase)
        || ex.InnerException is HttpRequestException;

    private static async Task<AuthOperationResult<AuthSession>> ReadSessionAsync(
        HttpResponseMessage response,
        string fallbackError)
    {
        if (response.IsSuccessStatusCode)
        {
            var session = await response.Content.ReadFromJsonAsync<AuthSession>();
            return session is null
                ? new AuthOperationResult<AuthSession>(null, fallbackError)
                : new AuthOperationResult<AuthSession>(session, null);
        }

        var error = await ReadErrorAsync(response) ?? fallbackError;
        return new AuthOperationResult<AuthSession>(null, error);
    }

    private static async Task<string?> ReadErrorAsync(HttpResponseMessage response)
    {
        try
        {
            var body = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
            return body?.Error;
        }
        catch
        {
            return null;
        }
    }

    private sealed record ApiErrorResponse(string? Error);
}
