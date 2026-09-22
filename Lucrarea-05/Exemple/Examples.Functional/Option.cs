using System.Diagnostics;

namespace Examples.Functional;

/// <summary>
/// O valoare care poate lipsi în mod legitim: <see cref="Some"/> sau <see cref="None"/> (F#: <c>Option&lt;'T&gt;</c>).
/// Spre deosebire de <c>T?</c>, absența nu poate fi ignorată cu <c>!</c>: apelantul trebuie să trateze ambele cazuri.
/// </summary>
public abstract record Option<T>
    where T : notnull
{
    private Option()
    {
    }

    /// <summary>Valoarea este prezentă.</summary>
    public sealed record Some(T Value) : Option<T>;

    /// <summary>Valoarea lipsește. Toate instanțele sunt egale între ele.</summary>
    public sealed record None : Option<T>;

    /// <summary>Reduce opțiunea la o singură valoare tratând ambele cazuri.</summary>
    public TResult Match<TResult>(Func<T, TResult> some, Func<TResult> none) => this switch
    {
        Some present => some(present.Value),
        None => none(),
        _ => throw new UnreachableException(),
    };

    /// <summary>Permite <c>Option&lt;Grade&gt; g = grade;</c>.</summary>
    public static implicit operator Option<T>(T value) => new Some(value);
}

/// <summary>Funcții de construcție pentru <see cref="Option{T}"/>.</summary>
public static class Option
{
    /// <summary>Opțiune cu valoare.</summary>
    public static Option<T> Some<T>(T value)
        where T : notnull
        => new Option<T>.Some(value);

    /// <summary>Opțiune fără valoare.</summary>
    public static Option<T> None<T>()
        where T : notnull
        => new Option<T>.None();

    /// <summary>Convertește o valoare nullable (de exemplu o coloană <c>decimal?</c> din baza de date) în opțiune.</summary>
    public static Option<T> FromNullable<T>(T? value)
        where T : struct
        => value is { } present ? new Option<T>.Some(present) : new Option<T>.None();
}
