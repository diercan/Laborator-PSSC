using System.Text.RegularExpressions;
using Examples.Functional;

namespace Examples.Domain.ValueObjects;

/// <summary>
/// Numărul matricol al unui student (format <c>LM12345</c>). Obiect-valoare cu un singur mod de eșec,
/// deci eroarea sa (<see cref="InvalidRegistrationNumberFormat"/>) nu are nevoie de o ierarhie închisă.
/// </summary>
public sealed partial record StudentRegistrationNumber
{
    /// <summary>Formatul acceptat: literele "LM" urmate de exact 5 cifre.</summary>
    public const string PatternText = "^LM[0-9]{5}$";

    [GeneratedRegex(PatternText)]
    private static partial Regex Pattern { get; }

    public string Value { get; }

    private StudentRegistrationNumber(string value) => Value = value;

    /// <summary>Construiește un număr matricol validat, sau întoarce motivul pentru care textul nu este valabil.</summary>
    public static Result<StudentRegistrationNumber, InvalidRegistrationNumberFormat> Create(string? raw) =>
        raw is not null && Pattern.IsMatch(raw)
            ? new StudentRegistrationNumber(raw)
            : new InvalidRegistrationNumberFormat(raw);

    public override string ToString() => Value;
}

/// <summary>Textul primit nu respectă formatul <see cref="StudentRegistrationNumber.PatternText"/>.</summary>
public sealed record InvalidRegistrationNumberFormat(string? Raw);
