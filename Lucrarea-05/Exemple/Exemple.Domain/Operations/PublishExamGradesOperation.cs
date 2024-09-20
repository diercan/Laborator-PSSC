using System;
using System.Linq;
using System.Text;
using static Examples.Domain.Models.ExamGrades;

namespace Examples.Domain.Operations
{
  internal class PublishExamGradesOperation
  {

    public IExamGrades PublishExamGrades(IExamGrades examGrades) => examGrades switch
    {
      CalculatedExamGrades calculatedExam => PublishExamGradesInternal(calculatedExam),

      UnvalidatedExamGrades unvalidatedExam => unvalidatedExam,
      InvalidExamGrades invalidExam => invalidExam,
      ValidatedExamGrades validatedExam => validatedExam,
      PublishedExamGrades publishedExam => publishedExam,
      FailedExamGrades failedExam => failedExam,

      _ => throw new System.NotImplementedException()
    };

    private PublishedExamGrades PublishExamGradesInternal(CalculatedExamGrades calculatedExam)
    {
      StringBuilder csv = new();
      calculatedExam.GradeList.Aggregate(csv, (export, grade) =>
        export.AppendLine($"{grade.StudentRegistrationNumber.Value}, {grade.ExamGrade}, {grade.ActivityGrade}, , {grade.FinalGrade}"));

      PublishedExamGrades publishedExamGrades = new(calculatedExam.GradeList, csv.ToString(), DateTime.Now);
      return publishedExamGrades;
    }
  }
}
