using System;
using System.Collections.Generic;

namespace Examples.Domain.Models
{
  public static partial class Exam
  {
    public interface IExam { }

    public record UnvalidatedExam(IReadOnlyCollection<UnvalidatedStudentGrade> GradesList) : IExam;

    public record InvalidatedExam(IReadOnlyCollection<UnvalidatedStudentGrade> GradesList, string Reason) : IExam;

    public record ValidatedExam(IReadOnlyCollection<ValidatedStudentGrade> GradesList) : IExam;

    public record PublishedExam(IReadOnlyCollection<ValidatedStudentGrade> GradesList, DateTime PublishedDate) : IExam;
  }
}
