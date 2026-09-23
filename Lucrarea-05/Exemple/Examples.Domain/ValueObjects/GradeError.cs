namespace Examples.Domain.ValueObjects;

/// <summary>Motivele pentru care un text sau un număr nu poate deveni o <see cref="Grade"/> validă.</summary>
public abstract record GradeError
{
    private GradeError()
    {
    }

    /// <summary>Textul primit nu reprezintă un număr.</summary>
    public sealed record NotANumber(string? Raw) : GradeError;

    /// <summary>Numărul este în afara intervalului (0, 10].</summary>
    public sealed record OutOfRange(decimal Value) : GradeError;
}
