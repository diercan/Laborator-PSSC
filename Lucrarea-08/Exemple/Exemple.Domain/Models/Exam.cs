using System;
using System.Collections.Generic;

namespace Examples.Domain.Models
{
  public static class Exam
  {
    public interface IExam { }

    public record UnvalidatedExam : IExam
    {
      public UnvalidatedExam(IReadOnlyCollection<UnvalidatedStudentGrade> gradeList)
      {
        GradeList = gradeList;
      }

      public IReadOnlyCollection<UnvalidatedStudentGrade> GradeList { get; }
    }

    public record InvalidExam : IExam
    {
      internal InvalidExam(IReadOnlyCollection<UnvalidatedStudentGrade> gradeList, IEnumerable<string> reasons)
      {
        GradeList = gradeList;
        Reasons = reasons;
      }

      public IReadOnlyCollection<UnvalidatedStudentGrade> GradeList { get; }
      public IEnumerable<string> Reasons { get; }
    }

    public record ValidatedExam : IExam
    {
      internal ValidatedExam(IReadOnlyCollection<ValidatedStudentGrade> gradesList)
      {
        GradeList = gradesList;
      }

      public IReadOnlyCollection<ValidatedStudentGrade> GradeList { get; }
    }

    public record CalculatedExam : IExam
    {
      internal CalculatedExam(IReadOnlyCollection<CalculatedStudentGrade> gradesList)
      {
        GradeList = gradesList;
      }

      public IReadOnlyCollection<CalculatedStudentGrade> GradeList { get; }
    }

    public record PublishedExam : IExam
    {
      internal PublishedExam(IReadOnlyCollection<CalculatedStudentGrade> gradesList, string csv, DateTime publishedDate)
      {
        GradeList = gradesList;
        PublishedDate = publishedDate;
        Csv = csv;
      }

      public IReadOnlyCollection<CalculatedStudentGrade> GradeList { get; }
      public DateTime PublishedDate { get; }
      public string Csv { get; }
    }
  }
}
