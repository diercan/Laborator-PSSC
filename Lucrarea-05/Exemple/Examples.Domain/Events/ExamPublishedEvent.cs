using Examples.Domain.States;

namespace Examples.Domain.Events;

/// <summary>
/// Evenimentul de domeniu produs de publicarea cu succes a unui examen (Wlaschin cap. 3: un eveniment descrie
/// un fapt petrecut). Nu există un eveniment de eșec: un eșec nu s-a întâmplat, deci nu este un eveniment —
/// este o eroare, transportată de <see cref="Examples.Functional.Result{TSuccess, TFailure}"/>.
/// </summary>
public sealed record ExamPublishedEvent(IReadOnlyList<CalculatedStudentGrade> Grades, string Csv, DateTimeOffset PublishedAt);
