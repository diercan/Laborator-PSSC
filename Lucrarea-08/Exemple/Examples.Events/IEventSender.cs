namespace Examples.Events;

/// <summary>Port prin care se publică un eveniment de integrare pe un topic.</summary>
public interface IEventSender
{
    /// <summary>Trimite <paramref name="event"/> pe topicul <paramref name="topic"/>.</summary>
    Task SendAsync<TEvent>(TopicName topic, TEvent @event, CancellationToken cancellationToken)
        where TEvent : IIntegrationEvent;
}
