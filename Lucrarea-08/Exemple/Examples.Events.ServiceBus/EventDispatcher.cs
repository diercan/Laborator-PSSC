using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;

namespace Examples.Events.ServiceBus;

/// <summary>Deserializează un <see cref="JsonElement"/> la <typeparamref name="TEvent"/> și îl predă unui <see cref="IEventHandler{TEvent}"/> dintr-un scope nou.</summary>
internal sealed class EventDispatcher<TEvent>(IServiceScopeFactory scopeFactory) : IEventDispatcher
    where TEvent : IIntegrationEvent
{
    public string EventType => TEvent.EventType;

    public async Task<EventProcessingResult> DispatchAsync(JsonElement payload, CancellationToken cancellationToken)
    {
        TEvent? @event = payload.Deserialize<TEvent>(JsonSerializerOptions.Web);
        if (@event is null)
        {
            // Mesaj cu formă corectă (CloudEvent valid) dar conținut ce nu se poate deserializa la TEvent:
            // date corupte, nu o eroare tranzitorie — se trimite direct în coada de mesaje moarte.
            return EventProcessingResult.Failed;
        }

        await using AsyncServiceScope scope = scopeFactory.CreateAsyncScope();
        IEventHandler<TEvent> handler = scope.ServiceProvider.GetRequiredService<IEventHandler<TEvent>>();
        return await handler.HandleAsync(@event, cancellationToken);
    }
}
