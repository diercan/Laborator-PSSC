namespace Examples.Events;

/// <summary>Numele unui topic de mesagerie. Obiect-valoare: previne confuzia cu alte șiruri de caractere (nume de subscripție, ID de mesaj).</summary>
public readonly record struct TopicName
{
    public string Value { get; }

    public TopicName(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        Value = value;
    }

    public override string ToString() => Value;
}
