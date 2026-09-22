namespace Examples.Functional;

/// <summary>
/// Tipul cu o singură valoare (F#: <c>unit</c>). Folosit ca <c>TSuccess</c> pentru operații care reușesc fără a produce
/// o valoare, de exemplu <c>Result&lt;Unit, SaveError&gt;</c>.
/// </summary>
public readonly record struct Unit
{
    /// <summary>Singura valoare posibilă.</summary>
    public static Unit Value => default;
}
