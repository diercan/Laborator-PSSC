using Examples.Domain.Models;
using System.Collections.Generic;
using System.Linq;
using static Examples.Domain.Models.ExamGrades;

namespace Examples.Domain.Operations
{
  internal class CalculateFinalGradesOperation
  {
    public CalculateFinalGradesOperation()
    {
    }

    public IExamGrades CalculateFinalGrades(IExamGrades examGrades, List<CalculatedStudentGrade> existingGrades) => examGrades switch
    {
      ValidatedExamGrades validExamGrades => CalculateFinalGradesInternal(validExamGrades, existingGrades),

      UnvalidatedExamGrades unvalidatedExam => unvalidatedExam,
      InvalidExamGrades invalidExam => invalidExam,
      CalculatedExamGrades calculatedExam => calculatedExam,
      PublishedExamGrades publishedExam => publishedExam,
      FailedExamGrades failedExam => failedExam,

      _ => throw new System.NotImplementedException()
    };

    private CalculatedExamGrades CalculateFinalGradesInternal(ValidatedExamGrades validExamGrades, List<CalculatedStudentGrade> existingGrades)
    {
      IEnumerable<CalculatedStudentGrade> calculatedGrade = validExamGrades.GradeList
        .Select(validGrade =>
        {
          CalculatedStudentGrade? existingGrade = existingGrades.FirstOrDefault(
            grade => grade.StudentRegistrationNumber.Equals(validGrade.StudentRegistrationNumber));
          return new CalculatedStudentGrade(
            validGrade.StudentRegistrationNumber,
            validGrade.ExamGrade,
            validGrade.ActivityGrade,
            validGrade.ExamGrade + validGrade.ActivityGrade)
          {
            GradeId = existingGrade?.GradeId ?? 0,
            IsUpdated = existingGrade is not null
          };
        });
      return new CalculatedExamGrades(calculatedGrade.ToList().AsReadOnly());
    }
  }
}
