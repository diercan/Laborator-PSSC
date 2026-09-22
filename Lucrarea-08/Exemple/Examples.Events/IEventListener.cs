namespace Examples.Events;

/// <summary>
/// Ascultă mesajele unei subscripții și le predă handler-ului potrivit. <see cref="StartAsync"/> trebuie să
/// fie idempotent (a doua chemare nu pornește un al doilea ascultător), iar implementarea trebuie eliberată
/// prin <see cref="IAsyncDisposable"/>.
/// </summary>
public interface IEventListener : IAsyncDisposable
{
    Task StartAsync(TopicName topic, SubscriptionName subscription, CancellationToken cancellationToken);

    Task StopAsync(CancellationToken cancellationToken);
}
