using System.Globalization;
using Examples.Functional;

namespace Examples.Domain.ValueObjects;

/// <summary>
/// O notă valabilă (examen, activitate sau nota finală): un număr în intervalul (0, 10], cu cel mult
/// două zecimale. Obiect-valoare (Wlaschin, cap. 5): imutabil, fără identitate, construit doar prin
/// <see cref="Create"/>/<see cref="Parse"/>, care întorc <see cref="Result{TSuccess, TFailure}"/> în loc să arunce excepții.
/// </summary>
public sealed record Grade
{
    /// <summary>Limita inferioară a unei note valabile (exclusiv).</summary>
    public const decimal Minimum = 0m;

    /// <summary>Limita superioară a unei note valabile (inclusiv).</summary>
    public const decimal Maximum = 10m;

    /// <summary>Pragul de promovare folosit la calculul notei finale.</summary>
    public const decimal PassingThreshold = 5m;

    /// <summary>Numărul de zecimale păstrate (coloanele din baza de date sunt <c>decimal(4,2)</c>).</summary>
    public const int Scale = 2;

    /// <summary>
    /// Valoarea numerică a notei, întotdeauna normalizată la <see cref="Scale"/> zecimale. Cuvântul cheie
    /// <c>field</c> (C# 14) garantează că normalizarea se aplică la orice scriere, inclusiv dintr-un <c>with</c>.
    /// </summary>
    public decimal Value
    {
        get;
        private init => field = Normalize(value);
    }

    private Grade(decimal value) => Value = value;

    /// <summary><see langword="true"/> dacă nota este suficientă pentru promovare.</summary>
    public bool IsPassing => Value >= PassingThreshold;

    /// <summary>Construiește o notă dintr-o valoare numerică deja cunoscută (de exemplu citită din baza de date).</summary>
    public static Result<Grade, GradeError> Create(decimal value) =>
        Normalize(value) is > Minimum and <= Maximum
            ? new Grade(value)
            : new GradeError.OutOfRange(value);

    /// <summary>
    /// Construiește o notă dintr-un text (consolă, formular web). Separatorul zecimal este întotdeauna
    /// punctul, indiferent de cultura mașinii: altfel, pe un calculator setat pe română, "7.5" ar fi interpretat ca 75.
    /// </summary>
    public static Result<Grade, GradeError> Parse(string? raw)
    {
        const NumberStyles styles = NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite;
        return decimal.TryParse(raw, styles, CultureInfo.InvariantCulture, out decimal value)
            ? Create(value)
            : new GradeError.NotANumber(raw);
    }

    /// <summary>Media a două note, rotunjită la <see cref="Scale"/> zecimale. Media a două note valabile este întotdeauna valabilă.</summary>
    public static Grade Average(Grade first, Grade second) => new((first.Value + second.Value) / 2m);

    public override string ToString() => Value.ToString("0.##", CultureInfo.InvariantCulture);

    private static decimal Normalize(decimal value) => decimal.Round(value, Scale, MidpointRounding.AwayFromZero);
}
