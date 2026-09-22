using Examples.Contracts.Reports;

namespace Examples.Api.Clients;

/// <summary>Client tipizat către <c>Examples.ReportGenerator</c>. Politica de reîncercare este configurată la înregistrare (vezi <see cref="ServiceCollectionExtensions"/>).</summary>
public sealed class ReportApiClient(HttpClient http)
{
    public Task<ReportAcknowledgement> GenerateSemesterReportAsync(ExamPublishedReport report, CancellationToken cancellationToken) =>
        PostAsync("report/semester-report", report, cancellationToken);

    public Task<ReportAcknowledgement> CalculateScholarshipAsync(ExamPublishedReport report, CancellationToken cancellationToken) =>
        PostAsync("report/scholarship", report, cancellationToken);

    private async Task<ReportAcknowledgement> PostAsync(string path, ExamPublishedReport report, CancellationToken cancellationToken)
    {
        using HttpResponseMessage response = await http.PostAsJsonAsync(path, report, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ReportAcknowledgement>(cancellationToken)
            ?? throw new InvalidOperationException($"Răspuns gol de la {path}.");
    }
}
