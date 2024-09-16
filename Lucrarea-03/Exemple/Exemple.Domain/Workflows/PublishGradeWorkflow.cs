using Examples.Domain.Commands;
using Examples.Domain.Models;
using Examples.Domain.Operations;
using System;
using static Examples.Domain.Models.ExamGrades;
using static Examples.Domain.Models.ExamGradesPublishedEvent;

namespace Examples.Domain.Workflows
{
  public class PublishGradeWorkflow
  {
    public IExamGradesPublishedEvent Execute(PublishGradesCommand command, Func<StudentRegistrationNumber, bool> checkStudentExists)
    {
      UnvalidatedExamGrades unvalidatedGrades = new(command.InputExamGrades);

      ValidateExamGradesOperation validateExamGrades = new(checkStudentExists);
      IExamGrades grades = validateExamGrades.ValidateExamGrades(unvalidatedGrades);

      CalculateFinalGradesOperation calculateFinalGrades = new();
      grades = calculateFinalGrades.CalculateFinalGrades(grades);

      PublishExamGradesOperation publishExamGrades = new();
      grades = publishExamGrades.PublishExamGrades(grades);


      return grades switch
      {
        UnvalidatedExamGrades _ => new ExamGradesPublishFailedEvent("Unexpected unvalidated state"),
        InvalidatedExamGrades invalidGrades => new ExamGradesPublishFailedEvent(invalidGrades.Reason),
        ValidatedExamGrades validatedGrades => new ExamGradesPublishFailedEvent("Unexpected validated state"),
        CalculatedExamGrades calculatedGrades => new ExamGradesPublishFailedEvent("Unexpected calculated state"),
        PublishedExamGrades publishedGrades => new ExamGradesPublishSucceededEvent(publishedGrades.Csv, publishedGrades.PublishedDate),
        _ => throw new NotImplementedException()
      };
    }
  }
}
