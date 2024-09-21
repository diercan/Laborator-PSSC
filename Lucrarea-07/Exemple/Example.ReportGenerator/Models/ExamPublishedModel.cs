namespace Examples.ReportGenerator.Models
{
  public record ExamPublishedModel
  {
    public string Csv { get; init; } = string.Empty;
    public DateTime PublishedDate { get; init; }
  }
}
