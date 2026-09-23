namespace Examples.Events;

/// <summary>
/// Un eveniment de integrare: date trimise unui alt context delimitat prin mesagerie asincronă.
/// <see cref="EventType"/> este un membru static abstract (C# 11): fiecare tip de eveniment își declară
/// propriul nume de rutare la compilare, în loc să se folosească reflecția (<c>typeof(T).Name</c>), care
/// s-ar rupe silențios dacă cineva redenumește clasa fără să știe că numele e parte din contract.
/// </summary>
public interface IIntegrationEvent
{
    /// <summary>Numele stabil, versionat, folosit pentru rutare (de exemplu <c>"upt.pssc.grades.published.v1"</c>).</summary>
    static abstract string EventType { get; }
}
