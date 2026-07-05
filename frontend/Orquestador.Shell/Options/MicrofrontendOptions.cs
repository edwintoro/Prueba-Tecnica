namespace Orquestador.Shell.Options;

public sealed class MicrofrontendOptions
{
    public const string SectionName = "Microfrontends";

    public string RegistroUrl { get; set; } = "/mf-registro";
    public string LoginUrl { get; set; } = "/mf-login";
    public bool AutoStart { get; set; } = true;
    public int LoginPort { get; set; } = 5102;
    public int RegistroPort { get; set; } = 5101;
}
