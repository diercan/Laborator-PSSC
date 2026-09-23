namespace Examples.Events;

/// <summary>Tratează un eveniment de integrare de un tip anume, primit de la un ascultător.</summary>
public interface IEventHandler<in TEvent>
    where TEvent : IIntegrationEvent
{
    Task<EventProcessingResult> HandleAsync(TEvent @event, CancellationToken cancellationToken);
}
