using System.Diagnostics;

namespace Examples.Domain.States;

/// <summary>
/// Tipul-sumă (choice type, Wlaschin cap. 6) care descrie stările prin care trece publicarea unui examen:
/// <see cref="Unvalidated"/> → <see cref="Validated"/> → <see cref="Calculated"/> → <see cref="Published"/>.
/// Ierarhia este închisă (constructor privat, înregistrări imbricate <see langword="sealed"/>), deci compilatorul
/// poate verifica exhaustivitatea unui <c>switch</c> asupra ei. Nu există o stare "Invalid": un eșec de validare
/// nu este o stare a examenului, ci o eroare transportată de <see cref="Examples.Functional.Result{TSuccess, TFailure}"/>
/// (vezi <see cref="Examples.Domain.Errors.PublishExamError"/>).
/// </summary>
public abstract record Exam
{
    private Exam()
    {
    }

    /// <summary>Notele așa cum au fost primite, înainte de validare.</summary>
    public sealed record Unvalidated(IReadOnlyList<UnvalidatedStudentGrade> Grades) : Exam;

    /// <summary>Notele validate: fiecare student există, fiecare notă este valabilă.</summary>
    public sealed record Validated(IReadOnlyList<ValidatedStudentGrade> Grades) : Exam;

    /// <summary>Notele validate, cu nota finală calculată pentru fiecare student.</summary>
    public sealed record Calculated(IReadOnlyList<CalculatedStudentGrade> Grades) : Exam;

    /// <summary>Rezultatul final: notele calculate, exportate ca CSV, cu momentul publicării.</summary>
    public sealed record Published(IReadOnlyList<CalculatedStudentGrade> Grades, string Csv, DateTimeOffset PublishedAt) : Exam;

    /// <summary>
    /// Reduce examenul la o singură valoare, tratând fiecare stare. Adăugarea unei stări noi în această clasă
    /// face ca fiecare apel al lui <see cref="Match{TResult}"/> din depozit să nu mai compileze, până este completat.
    /// </summary>
    public TResult Match<TResult>(
        Func<Unvalidated, TResult> unvalidated,
        Func<Validated, TResult> validated,
        Func<Calculated, TResult> calculated,
        Func<Published, TResult> published) => this switch
        {
            Unvalidated exam => unvalidated(exam),
            Validated exam => validated(exam),
            Calculated exam => calculated(exam),
            Published exam => published(exam),
            _ => throw new UnreachableException(),
        };
}
