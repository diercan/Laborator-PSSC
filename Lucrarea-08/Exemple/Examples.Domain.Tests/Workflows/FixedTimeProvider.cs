namespace Examples.Domain.Tests.Workflows;

/// <summary>Un ceas care întoarce mereu aceeași valoare, pentru teste deterministe.</summary>
internal sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
{
    public override DateTimeOffset GetUtcNow() => now;
}
