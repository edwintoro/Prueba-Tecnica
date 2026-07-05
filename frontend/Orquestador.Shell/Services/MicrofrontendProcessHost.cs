using System.Diagnostics;
using Microsoft.Extensions.Options;
using Orquestador.Shell.Options;

namespace Orquestador.Shell.Services;

public sealed class MicrofrontendProcessHost : IHostedService, IDisposable
{
    private readonly IWebHostEnvironment _env;
    private readonly MicrofrontendOptions _options;
    private readonly ILogger<MicrofrontendProcessHost> _logger;
    private readonly List<Process> _processes = [];

    public MicrofrontendProcessHost(
        IWebHostEnvironment env,
        IOptions<MicrofrontendOptions> options,
        ILogger<MicrofrontendProcessHost> logger)
    {
        _env = env;
        _options = options.Value;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (!_env.IsDevelopment() || !_options.AutoStart)
            return;

        var frontendDir = Directory.GetParent(_env.ContentRootPath)?.FullName;
        if (frontendDir is null)
        {
            _logger.LogWarning("No se encontró la carpeta frontend; microfrontends no se iniciarán automáticamente.");
            return;
        }

        await StartIfNeededAsync(
            "Microfrontend.Login",
            Path.Combine(frontendDir, "Microfrontend.Login", "Microfrontend.Login.csproj"),
            _options.LoginPort,
            cancellationToken);

        await StartIfNeededAsync(
            "Microfrontend.Registro",
            Path.Combine(frontendDir, "Microfrontend.Registro", "Microfrontend.Registro.csproj"),
            _options.RegistroPort,
            cancellationToken);
    }

    private async Task StartIfNeededAsync(
        string name,
        string projectPath,
        int port,
        CancellationToken cancellationToken)
    {
        if (await IsReachableAsync(port, cancellationToken))
        {
            _logger.LogInformation("{Name} ya está activo en el puerto {Port}.", name, port);
            return;
        }

        if (!File.Exists(projectPath))
        {
            _logger.LogWarning("No se encontró {Project}; inicia {Name} manualmente.", projectPath, name);
            return;
        }

        _logger.LogInformation("Iniciando {Name} en el puerto {Port}...", name, port);

        var process = Process.Start(new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = $"run --project \"{projectPath}\"",
            WorkingDirectory = Path.GetDirectoryName(projectPath)!,
            UseShellExecute = false,
            CreateNoWindow = true
        });

        if (process is null)
        {
            _logger.LogError("No se pudo iniciar {Name}.", name);
            return;
        }

        _processes.Add(process);

        if (!await WaitUntilReadyAsync(port, cancellationToken))
            _logger.LogWarning("{Name} no respondió a tiempo en el puerto {Port}.", name, port);
        else
            _logger.LogInformation("{Name} listo en http://localhost:{Port}.", name, port);
    }

    private static async Task<bool> IsReachableAsync(int port, CancellationToken cancellationToken)
    {
        try
        {
            using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(2) };
            using var response = await client.GetAsync($"http://localhost:{port}", cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    private static async Task<bool> WaitUntilReadyAsync(int port, CancellationToken cancellationToken)
    {
        for (var i = 0; i < 60; i++)
        {
            if (await IsReachableAsync(port, cancellationToken))
                return true;

            await Task.Delay(500, cancellationToken);
        }

        return false;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        foreach (var process in _processes)
        {
            try
            {
                if (!process.HasExited)
                    process.Kill(entireProcessTree: true);
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Error al detener un microfrontend.");
            }
            finally
            {
                process.Dispose();
            }
        }

        _processes.Clear();
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        foreach (var process in _processes)
            process.Dispose();
    }
}
