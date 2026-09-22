namespace Examples.Events.ServiceBus;

/// <summary>Opțiuni pentru trimiterea evenimentelor peste Azure Service Bus, legate din secțiunea <see cref="SectionName"/> din configurație.</summary>
public sealed class ServiceBusEventsOptions
{
    public const string SectionName = "ServiceBusEvents";

    /// <summary>Sursa (<c>CloudEvent.Source</c>) atribuită evenimentelor trimise de acest serviciu.</summary>
    public Uri Source { get; set; } = new("urn:pssc:examples-api");
}
