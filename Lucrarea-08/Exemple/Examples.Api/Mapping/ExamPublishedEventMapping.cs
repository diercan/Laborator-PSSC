using Examples.Contracts.Events;
using Examples.Contracts.Models;
using Examples.Domain.Events;

namespace Examples.Api.Mapping;

/// <summary>Conversia evenimentului de domeniu (intern) în evenimentul de integrare (contractul public, partajat pe fir).</summary>
public static class ExamPublishedEventMapping
{
    extension(ExamPublishedEvent published)
    {
        public GradesPublishedEvent ToIntegrationEvent() => new(
            published.PublishedAt,
            [.. published.Grades.Select(grade => new StudentGradeDto(
                grade.RegistrationNumber.Value,
                grade.ExamGrade.Value,
                grade.ActivityGrade.Value,
                grade.FinalGrade.Match(final => (decimal?)final.Value, () => null)))]);
    }
}
