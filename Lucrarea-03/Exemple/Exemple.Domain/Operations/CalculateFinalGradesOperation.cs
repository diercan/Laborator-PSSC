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

    public IExamGrades CalculateFinalGrades(IExamGrades examGrades) => examGrades switch
    {
      ValidatedExamGrades validExamGrades => CalculateFinalGradesInternal(validExamGrades),

      UnvalidatedExamGrades unvalidatedExam => unvalidatedExam,
      InvalidatedExamGrades invalidExam => invalidExam,
      CalculatedExamGrades calculatedExam => calculatedExam,
      PublishedExamGrades publishedExam => publishedExam,

      _ => throw new System.NotImplementedException()
    };

    private CalculatedExamGrades CalculateFinalGradesInternal(ValidatedExamGrades validExamGrades)
    {
      IEnumerable<CalculatedStudentGrade> calculatedGrade = validExamGrades.GradeList
        .Select(validGrade =>
          new CalculatedStudentGrade(
            validGrade.StudentRegistrationNumber,
            validGrade.ExamGrade,
            validGrade.ActivityGrade,
            validGrade.ExamGrade + validGrade.ActivityGrade));
      return new CalculatedExamGrades(calculatedGrade.ToList().AsReadOnly());
    }
  }
}
