using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using Orquestador.Shell.Options;

namespace Orquestador.Shell.Pages;

public sealed class LoginModel : PageModel
{
    private readonly MicrofrontendOptions _options;

    public LoginModel(IOptions<MicrofrontendOptions> options) =>
        _options = options.Value;

    public string LoginUrl => _options.LoginUrl;
}
