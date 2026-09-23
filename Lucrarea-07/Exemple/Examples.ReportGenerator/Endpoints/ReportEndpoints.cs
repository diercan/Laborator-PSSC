using Examples.Contracts.Reports;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Examples.ReportGenerator.Endpoints;

/// <summary>
/// Contextul de raportare: primește notele publicate și produce rapoarte. Exemplu minimal — doar
/// confirmă primirea; o implementare reală ar genera aici raportul semestrial și ar calcula bursele.
/// </summary>
public static partial class ReportEndpoints
{
    public static IEndpointRouteBuilder MapReports(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/report").WithTags("Report");

        group.MapPost("/semester-report", GenerateSemesterReportAsync).WithName("GenerateSemesterReport");
        group.MapPost("/scholarship", CalculateScholarshipAsync).WithName("CalculateScholarship");

        return app;
    }

    private static Ok<ReportAcknowledgement> GenerateSemesterReportAsync(
        ExamPublishedReport report, ILogger<Program> logger, TimeProvider clock)
    {
        LogSemesterReportGenerated(logger, report.PublishedAt);
        return TypedResults.Ok(new ReportAcknowledgement("Semester report generated.", clock.GetUtcNow()));
    }

    private static Ok<ReportAcknowledgement> CalculateScholarshipAsync(
        ExamPublishedReport report, ILogger<Program> logger, TimeProvider clock)
    {
        LogScholarshipCalculated(logger, report.PublishedAt);
        return TypedResults.Ok(new ReportAcknowledgement("Scholarship calculation completed.", clock.GetUtcNow()));
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Generated semester report for the catalog published at {PublishedAt}")]
    private static partial void LogSemesterReportGenerated(ILogger logger, DateTimeOffset publishedAt);

    [LoggerMessage(Level = LogLevel.Information, Message = "Calculated scholarships for the catalog published at {PublishedAt}")]
    private static partial void LogScholarshipCalculated(ILogger logger, DateTimeOffset publishedAt);
}
