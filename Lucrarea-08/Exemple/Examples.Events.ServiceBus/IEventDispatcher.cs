using System.Text.Json;

namespace Examples.Events.ServiceBus;

/// <summary>
/// Trimite conținutul unui mesaj CloudEvents deja decodat către handler-ul potrivit tipului. Rămâne intern
/// pachetului: restul aplicației lucrează cu <see cref="IEventHandler{TEvent}"/>, nu vede JSON-ul brut.
/// </summary>
internal interface IEventDispatcher
{
    string EventType { get; }

    Task<EventProcessingResult> DispatchAsync(JsonElement payload, CancellationToken cancellationToken);
}
