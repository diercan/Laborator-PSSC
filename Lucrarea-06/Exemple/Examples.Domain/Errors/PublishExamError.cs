using System.Diagnostics;

namespace Examples.Domain.Errors;

/// <summary>Motivul pentru care workflow-ul de publicare a notelor unui examen nu a produs un eveniment.</summary>
public abstract record PublishExamError
{
    private PublishExamError()
    {
    }

    /// <summary>Una sau mai multe note nu au trecut validarea.</summary>
    public sealed record Validation(IReadOnlyList<ValidationError> Errors) : PublishExamError;

    /// <summary>Mesajele tuturor erorilor, potrivite pentru afișare (consolă sau răspuns HTTP).</summary>
    public IReadOnlyList<string> ToMessages() => this switch
    {
        Validation v => [.. v.Errors.Select(e => e.ToMessage())],
        _ => throw new UnreachableException(),
    };

    // "sealed": fără el, fiecare înregistrare derivată (aici Validation) își sintetizează propriul ToString()
    // care ascunde acest override — vezi https://learn.microsoft.com/dotnet/csharp/fundamentals/tutorials/records#formatting
    public sealed override string ToString() => string.Join(Environment.NewLine, ToMessages());
}
