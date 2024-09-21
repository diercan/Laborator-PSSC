using Examples.Domain.Models;
using System;
using System.Linq;
using System.Text;
using static Examples.Domain.Models.Exam;

namespace Examples.Domain.Operations
{
  internal sealed class PublishExamOperation : ExamOperation
  {
    protected override IExam OnCalculated(CalculatedExam calculatedExam)
    {
      StringBuilder csv = new();
      calculatedExam.GradeList.Aggregate(csv, (export, grade) =>
        export.AppendLine(GenerateCsvLine(grade)));

      PublishedExam publishedExamGrades = new(calculatedExam.GradeList, csv.ToString(), DateTime.Now);
      return publishedExamGrades;
    }

    private static string GenerateCsvLine(CalculatedStudentGrade grade) =>
      $"{grade.StudentRegistrationNumber.Value}, {grade.ExamGrade}, {grade.ActivityGrade}, {grade.FinalGrade}";
  }
}
