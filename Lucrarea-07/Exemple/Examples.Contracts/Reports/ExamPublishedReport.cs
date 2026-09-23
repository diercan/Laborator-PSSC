namespace Examples.Contracts.Reports;

/// <summary>Datele trimise către <c>Examples.ReportGenerator</c> după publicarea cu succes a unui catalog de note.</summary>
public sealed record ExamPublishedReport(string Csv, DateTimeOffset PublishedAt);
