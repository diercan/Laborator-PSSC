using System.ComponentModel.DataAnnotations;

namespace Examples.Api.Clients;

/// <summary>Configurarea clientului HTTP către <c>Examples.ReportGenerator</c>, legată din secțiunea <see cref="SectionName"/>.</summary>
public sealed class ReportApiOptions
{
    public const string SectionName = "ReportApi";

    [Required]
    public Uri BaseAddress { get; set; } = null!;

    public RetryOptions Retry { get; set; } = new();

    public sealed class RetryOptions
    {
        [Range(0, 10)]
        public int MaxRetryAttempts { get; set; } = 3;

        [Range(50, 10_000)]
        public int BaseDelayMilliseconds { get; set; } = 600;
    }
}
