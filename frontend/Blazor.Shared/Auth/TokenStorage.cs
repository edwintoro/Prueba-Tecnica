namespace Blazor.Shared.Auth;

public interface ITokenStorage
{
    AuthSession? Current { get; }
    void Set(AuthSession session);
    void Clear();
    bool IsAuthenticated { get; }
    event Action? Changed;
}

public sealed class TokenStorage : ITokenStorage
{
    private AuthSession? _current;

    public AuthSession? Current => _current;
    public bool IsAuthenticated => _current is not null && !string.IsNullOrWhiteSpace(_current.AccessToken);

    public event Action? Changed;

    public void Set(AuthSession session)
    {
        _current = session;
        Changed?.Invoke();
    }

    public void Clear()
    {
        _current = null;
        Changed?.Invoke();
    }
}
