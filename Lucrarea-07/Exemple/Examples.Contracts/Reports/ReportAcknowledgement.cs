namespace Examples.Contracts.Reports;

/// <summary>Confirmarea întoarsă de <c>Examples.ReportGenerator</c> pentru o cerere primită.</summary>
public sealed record ReportAcknowledgement(string Message, DateTimeOffset ReceivedAt);
