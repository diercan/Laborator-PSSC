using System;

namespace Examples.Domain.WorkflowEvents
{
  public static partial class ExamGradesPublishedEvent
  {
    public interface IExamGradesPublishedEvent { }

    public record ExamGradesPublishSucceededEvent : IExamGradesPublishedEvent
    {
      public string Csv { get; }
      public DateTime PublishedDate { get; }

      internal ExamGradesPublishSucceededEvent(string csv, DateTime publishedDate)
      {
        Csv = csv;
        PublishedDate = publishedDate;
      }
    }

    public record ExamGradesPublishFailedEvent : IExamGradesPublishedEvent
    {
      public string Reason { get; }

      internal ExamGradesPublishFailedEvent(string reason)
      {
        Reason = reason;
      }
    }
  }
}
