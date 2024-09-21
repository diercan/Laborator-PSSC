using System;
using System.Collections.Generic;
using static Examples.Domain.Models.Exam;

namespace Examples.Domain.Models
{
  public static class ExamPublishedEvent
  {
    public interface IExamPublishedEvent { }

    public record ExamPublishSucceededEvent : IExamPublishedEvent
    {
      public string Csv { get; }
      public DateTime PublishedDate { get; }

      internal ExamPublishSucceededEvent(string csv, DateTime publishedDate)
      {
        Csv = csv;
        PublishedDate = publishedDate;
      }
    }

    public record ExamPublishFailedEvent : IExamPublishedEvent
    {
      public IEnumerable<string> Reasons { get; }

      internal ExamPublishFailedEvent(string reason)
      {
        Reasons = [reason];
      }

      internal ExamPublishFailedEvent(IEnumerable<string> reasons)
      {
        Reasons = reasons;
      }
    }

    public static IExamPublishedEvent ToEvent(this IExam exam) =>
      exam switch
      {
        UnvalidatedExam _ => new ExamPublishFailedEvent("Unexpected unvalidated state"),
        ValidatedExam validatedGrades => new ExamPublishFailedEvent("Unexpected validated state"),
        CalculatedExam calculatedGrades => new ExamPublishFailedEvent("Unexpected calculated state"),
        InvalidExam invalidGrades => new ExamPublishFailedEvent(invalidGrades.Reasons),
        PublishedExam publishedGrades => new ExamPublishSucceededEvent(publishedGrades.Csv, publishedGrades.PublishedDate),
        _ => throw new NotImplementedException()
      };
  }
}
