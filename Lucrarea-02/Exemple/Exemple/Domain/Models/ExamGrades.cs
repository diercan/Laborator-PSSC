using System;
using System.Collections.Generic;

namespace Examples.Domain.Models
{
  public static partial class ExamGrades
  {
    public interface IExamGrades { }

    public record UnvalidatedExamGrades(IReadOnlyCollection<UnvalidatedStudentGrade> GradesList) : IExamGrades;

    public record InvalidatedExamGrades(IReadOnlyCollection<UnvalidatedStudentGrade> GradesList, string Reason) : IExamGrades;

    public record ValidatedExamGrades(IReadOnlyCollection<ValidatedStudentGrade> GradesList) : IExamGrades;

    public record PublishedExamGrades(IReadOnlyCollection<ValidatedStudentGrade> GradesList, DateTime PublishedDate) : IExamGrades;
  }
}
