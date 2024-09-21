using Examples.Domain.Models;
using System.Collections.Generic;
using System.Linq;
using static Examples.Domain.Models.Exam;

namespace Examples.Domain.Operations
{
  internal sealed class CalculateExamOperation : ExamOperation
  {
    internal CalculateExamOperation()
    {
    }

    protected override IExam OnValid(ValidatedExam validExamGrades)
    {
      IEnumerable<CalculatedStudentGrade> calculatedGrade = validExamGrades.GradeList
        .Select(validGrade =>
          new CalculatedStudentGrade(
            validGrade.StudentRegistrationNumber,
            validGrade.ExamGrade,
            validGrade.ActivityGrade,
            CalculateFinalGrade(validGrade)));
      return new CalculatedExam(calculatedGrade.ToList().AsReadOnly());
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
