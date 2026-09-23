using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Options;
using Polly;

namespace Examples.Api.Clients;

/// <summary>Înregistrează <see cref="ReportApiClient"/> cu 3 reîncercări, cu întârziere exponențială și jitter (cerința din README).</summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddReportApiClient(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddOptions<ReportApiOptions>()
            .Bind(configuration.GetSection(ReportApiOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services
            .AddHttpClient<ReportApiClient>((provider, client) =>
                client.BaseAddress = provider.GetRequiredService<IOptions<ReportApiOptions>>().Value.BaseAddress)
            .AddResilienceHandler("report-api", (pipeline, context) =>
            {
                ReportApiOptions.RetryOptions retry = context.ServiceProvider.GetRequiredService<IOptions<ReportApiOptions>>().Value.Retry;
                pipeline.AddRetry(new HttpRetryStrategyOptions
                {
                    MaxRetryAttempts = retry.MaxRetryAttempts,
                    BackoffType = DelayBackoffType.Exponential,
                    UseJitter = true,
                    Delay = TimeSpan.FromMilliseconds(retry.BaseDelayMilliseconds),
                });
                pipeline.AddTimeout(TimeSpan.FromSeconds(5));
            });

        return services;
    }
}
