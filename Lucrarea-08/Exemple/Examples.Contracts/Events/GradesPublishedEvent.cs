using Examples.Contracts.Models;
using Examples.Events;

namespace Examples.Contracts.Events;

/// <summary>
/// Evenimentul de integrare publicat de <c>Examples.Api</c> către celelalte contexte (de exemplu cazarea)
/// după publicarea cu succes a notelor unui examen. Un contract simplu, fără logică: doar date.
/// </summary>
public sealed record GradesPublishedEvent(DateTimeOffset PublishedAt, IReadOnlyList<StudentGradeDto> Grades) : IIntegrationEvent
{
    public static string EventType => "pssc.grades.published.v1";
}
