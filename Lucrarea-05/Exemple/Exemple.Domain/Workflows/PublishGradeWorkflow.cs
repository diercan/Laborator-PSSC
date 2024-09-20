using Examples.Domain.Models;
using Examples.Domain.Operations;
using Examples.Domain.Repositories;
using Exemple.Domain.Repositories;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Examples.Domain.Models.ExamGrades;
using static Examples.Domain.WorkflowEvents.ExamGradesPublishedEvent;

namespace Examples.Domain.Workflows
{
  public class PublishGradeWorkflow
  {
    private readonly IStudentsRepository studentsRepository;
    private readonly IGradesRepository gradesRepository;
    private readonly ILogger<PublishGradeWorkflow> logger;

    public PublishGradeWorkflow(IStudentsRepository studentsRepository, IGradesRepository gradesRepository, ILogger<PublishGradeWorkflow> logger)
    {
      this.studentsRepository = studentsRepository;
      this.gradesRepository = gradesRepository;
      this.logger = logger;
    }

    public async Task<IExamGradesPublishedEvent> ExecuteAsync(PublishGradesCommand command)
    {
      try
      {
        //load state from database
        IEnumerable<string> studentsToCheck = command.InputExamGrades.Select(grade => grade.StudentRegistrationNumber);
        List<StudentRegistrationNumber> existingStudents = await studentsRepository.GetExistingStudentsAsync(studentsToCheck);
        List<CalculatedStudentGrade> existingGrades = await gradesRepository.GetExistingGradesAsync();

        //execute pure business logic
        IExamGrades grades = ExecuteBusinessLogic(command, existingStudents, existingGrades);

        //save new state to database
        if (grades is PublishedExamGrades publishedGrades)
        {
          await gradesRepository.SaveGradesAsync(publishedGrades);
        }

        //evaluate the state of the entity and generate the appropriate event
        return ConvertToEvent(grades);
      }
      catch (Exception ex)
      {
        logger.LogError(ex, "An error occurred while publishing grades");
        return new ExamGradesPublishFailedEvent(ex.Message);
      }
    }

    private static IExamGrades ExecuteBusinessLogic(
      PublishGradesCommand command,
      List<StudentRegistrationNumber> existingStudents,
      List<CalculatedStudentGrade> existingGrades)
    {
      //start with unvalidated state
      UnvalidatedExamGrades unvalidatedGrades = new(command.InputExamGrades);

      //perform all business operations, operations are pure functions,
      //they could also be injected as dependencies in the constructor
      Func<StudentRegistrationNumber, bool> checkStudentExists = student => existingStudents.Any(s => s.Equals(student));
      ValidateExamGradesOperation validateExamGrades = new(checkStudentExists);
      IExamGrades grades = validateExamGrades.ValidateExamGrades(unvalidatedGrades);

      CalculateFinalGradesOperation calculateFinalGrades = new();
      grades = calculateFinalGrades.CalculateFinalGrades(grades, existingGrades);

      PublishExamGradesOperation publishExamGrades = new();
      grades = publishExamGrades.PublishExamGrades(grades);
      return grades;
    }

    private static IExamGradesPublishedEvent ConvertToEvent(IExamGrades grades)
    {
      return grades switch
      {
        UnvalidatedExamGrades _ => new ExamGradesPublishFailedEvent("Unexpected unvalidated state"),
        InvalidExamGrades invalidGrades => new ExamGradesPublishFailedEvent(invalidGrades.Reason),
        ValidatedExamGrades validatedGrades => new ExamGradesPublishFailedEvent("Unexpected validated state"),
        CalculatedExamGrades calculatedGrades => new ExamGradesPublishFailedEvent("Unexpected calculated state"),
        PublishedExamGrades publishedGrades => new ExamGradesPublishSucceededEvent(publishedGrades.Csv, publishedGrades.PublishedDate),
        FailedExamGrades failedGrades => new ExamGradesPublishFailedEvent(failedGrades.Exception.Message),
        _ => throw new NotImplementedException()
      };
    }
  }
}
