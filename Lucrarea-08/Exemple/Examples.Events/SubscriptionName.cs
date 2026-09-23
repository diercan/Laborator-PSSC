namespace Examples.Events;

/// <summary>Numele unei subscripții la un topic. Obiect-valoare, ca și <see cref="TopicName"/>.</summary>
public readonly record struct SubscriptionName
{
    public string Value { get; }

    public SubscriptionName(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        Value = value;
    }

    public override string ToString() => Value;
}
