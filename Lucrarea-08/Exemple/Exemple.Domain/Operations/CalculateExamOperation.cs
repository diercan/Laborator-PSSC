using Examples.Domain.Models;
using System.Collections.Generic;
using System.Linq;
using static Examples.Domain.Models.Exam;

namespace Examples.Domain.Operations
{
  internal sealed class CalculateExamOperation : ExamOperation<List<CalculatedStudentGrade>>
  {
    internal CalculateExamOperation()
    {
    }

    protected override IExam OnValid(ValidatedExam validExamGrades, List<CalculatedStudentGrade>? existingGrades) =>
      new CalculatedExam(
        validExamGrades.GradeList
          .Select(validGrade => CalculateAndMatchGrade(existingGrades, validGrade))
          .ToList()
          .AsReadOnly());

    private static CalculatedStudentGrade CalculateAndMatchGrade(List<CalculatedStudentGrade>? existingGrades, ValidatedStudentGrade validGrade)
    {
      CalculatedStudentGrade? existingGrade = existingGrades?.FirstOrDefault(
                  grade => grade.StudentRegistrationNumber.Equals(validGrade.StudentRegistrationNumber));
      Grade? finalGrade = CalculateFinalGrade(validGrade);
      return new CalculatedStudentGrade(
                  validGrade.StudentRegistrationNumber,
                  validGrade.ExamGrade,
                  validGrade.ActivityGrade,
                  finalGrade)
      {
        GradeId = existingGrade?.GradeId ?? 0,
        IsUpdated = existingGrade is not null
      };
    }

    private static Grade? CalculateFinalGrade(ValidatedStudentGrade validGrade)
    {
      return validGrade.ExamGrade is not null
             && validGrade.ExamGrade.Value >= 5
             && validGrade.ActivityGrade is not null
             && validGrade.ActivityGrade.Value >= 5
             ? validGrade.ExamGrade + validGrade.ActivityGrade
             : null;
    }
  }
}
