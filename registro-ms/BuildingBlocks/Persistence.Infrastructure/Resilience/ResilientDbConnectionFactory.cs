using System.Data.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Persistence.Abstractions;
using Polly;
using Polly.CircuitBreaker;
using Polly.Registry;
using Polly.Retry;

namespace Persistence.Infrastructure.Resilience;

public sealed class ResilienceOptions
{
    public const string SectionName = "DatabaseResilience";
    public int MaxRetryAttempts { get; set; } = 3;
    public int BaseDelaySeconds { get; set; } = 3;
    public int CircuitBreakerFailures { get; set; } = 5;
    public int CircuitBreakerDurationSeconds { get; set; } = 30;
}

public sealed class ResilientDbConnectionFactory : IDbConnectionFactory
{
    private readonly DatabaseConnectionFactory _inner;
    private readonly ResiliencePipeline _pipeline;
    private readonly ILogger<ResilientDbConnectionFactory> _logger;

    public ResilientDbConnectionFactory(
        DatabaseConnectionFactory inner,
        ResiliencePipelineProvider<string> pipelineProvider,
        ILogger<ResilientDbConnectionFactory> logger)
    {
        _inner = inner;
        _pipeline = pipelineProvider.GetPipeline("database");
        _logger = logger;
    }

    public DatabaseProvider Provider => _inner.Provider;

    public async Task<DbConnection> CreateOpenConnectionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _pipeline.ExecuteAsync(
                async token => await _inner.CreateOpenConnectionAsync(token),
                cancellationToken);
        }
        catch (BrokenCircuitException ex)
        {
            _logger.LogWarning(ex, "Circuit breaker abierto para base de datos.");
            throw new DatabaseUnavailableException("Base de datos no disponible. Intente más tarde.", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fallo de base de datos tras reintentos.");
            throw new DatabaseUnavailableException("Base de datos no disponible.", ex);
        }
    }
}

public static class ResilienceServiceCollectionExtensions
{
    public static IServiceCollection AddDatabaseResilience(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var section = configuration.GetSection(ResilienceOptions.SectionName);
        services.Configure<ResilienceOptions>(section);
        var opts = section.Get<ResilienceOptions>() ?? new ResilienceOptions();

        services.AddResiliencePipeline("database", builder =>
        {
            builder.AddRetry(new RetryStrategyOptions
            {
                MaxRetryAttempts = opts.MaxRetryAttempts,
                Delay = TimeSpan.FromSeconds(opts.BaseDelaySeconds),
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true,
                ShouldHandle = new PredicateBuilder().Handle<Exception>(ex => ex is not DatabaseUnavailableException)
            });
            builder.AddCircuitBreaker(new CircuitBreakerStrategyOptions
            {
                FailureRatio = 1.0,
                MinimumThroughput = opts.CircuitBreakerFailures,
                BreakDuration = TimeSpan.FromSeconds(opts.CircuitBreakerDurationSeconds),
                ShouldHandle = new PredicateBuilder().Handle<Exception>()
            });
        });

        services.AddSingleton<DatabaseConnectionFactory>();
        services.AddSingleton<IDbConnectionFactory, ResilientDbConnectionFactory>();
        return services;
    }
}
