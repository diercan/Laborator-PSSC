using System.Diagnostics;
using Examples.Domain.ValueObjects;

namespace Examples.Domain.Errors;

/// <summary>Un motiv pentru care o notă introdusă nu a putut fi validată.</summary>
public abstract record ValidationError
{
    private ValidationError()
    {
    }

    /// <summary>Numărul matricol nu respectă formatul așteptat.</summary>
    public sealed record InvalidRegistrationNumber(string? Raw) : ValidationError;

    /// <summary>Numărul matricol are formatul corect, dar nu aparține niciunui student cunoscut.</summary>
    public sealed record StudentNotFound(StudentRegistrationNumber RegistrationNumber) : ValidationError;

    /// <summary>Același număr matricol apare de mai multe ori în aceeași cerere de publicare.</summary>
    public sealed record DuplicateRegistrationNumber(StudentRegistrationNumber RegistrationNumber) : ValidationError;

    /// <summary>Nota de la examen nu este un număr valabil.</summary>
    public sealed record InvalidExamGrade(string? RegistrationNumber, GradeError Error) : ValidationError;

    /// <summary>Nota de la activitate nu este un număr valabil.</summary>
    public sealed record InvalidActivityGrade(string? RegistrationNumber, GradeError Error) : ValidationError;

    /// <summary>Cod stabil, potrivit pentru gruparea erorilor într-un răspuns HTTP (numele tipului concret).</summary>
    public string Code => GetType().Name;

    /// <summary>Un mesaj scurt, în limba engleză, potrivit pentru afișare directă.</summary>
    public string ToMessage() => this switch
    {
        InvalidRegistrationNumber e => $"Invalid student registration number ({e.Raw})",
        StudentNotFound e => $"Student not found ({e.RegistrationNumber})",
        DuplicateRegistrationNumber e => $"Duplicate student registration number ({e.RegistrationNumber})",
        InvalidExamGrade e => $"Invalid exam grade ({e.RegistrationNumber}, {DescribeGradeError(e.Error)})",
        InvalidActivityGrade e => $"Invalid activity grade ({e.RegistrationNumber}, {DescribeGradeError(e.Error)})",
        _ => throw new UnreachableException(),
    };

    private static string DescribeGradeError(GradeError error) => error switch
    {
        GradeError.NotANumber e => e.Raw ?? "<missing>",
        GradeError.OutOfRange e => e.Value.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture),
        _ => throw new UnreachableException(),
    };
}
