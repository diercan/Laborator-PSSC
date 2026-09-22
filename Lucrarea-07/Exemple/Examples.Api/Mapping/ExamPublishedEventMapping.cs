using Examples.Contracts.Reports;
using Examples.Domain.Events;

namespace Examples.Api.Mapping;

/// <summary>Conversia evenimentului de domeniu (intern) în datele trimise către contextul de raportare.</summary>
public static class ExamPublishedEventMapping
{
    extension(ExamPublishedEvent published)
    {
        public ExamPublishedReport ToReport() => new(published.Csv, published.PublishedAt);
    }
}
