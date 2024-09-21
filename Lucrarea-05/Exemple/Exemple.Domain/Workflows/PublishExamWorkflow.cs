using Examples.Domain.Models;
using Examples.Domain.Operations;
using Examples.Domain.Repositories;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Examples.Domain.Models.Exam;
using static Examples.Domain.Models.ExamPublishedEvent;

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

    public async Task<IExamPublishedEvent> ExecuteAsync(PublishExamCommand command)
    {
      try
      {
        //load state from database
        IEnumerable<string> studentsToCheck = command.InputExamGrades.Select(grade => grade.StudentRegistrationNumber);
        List<StudentRegistrationNumber> existingStudents = await studentsRepository.GetExistingStudentsAsync(studentsToCheck);
        List<CalculatedStudentGrade> existingGrades = await gradesRepository.GetExistingGradesAsync();

        //execute pure business logic
        IExam exam = ExecuteBusinessLogic(command, existingStudents, existingGrades);

        //save new state to database
        if (exam is PublishedExam publishedExam)
        {
          await gradesRepository.SaveGradesAsync(publishedExam);
        }

        //evaluate the state of the entity and generate the appropriate event
        return exam.ToEvent();
      }
      catch (Exception ex)
      {
        logger.LogError(ex, "An error occurred while publishing grades");
        return new ExamPublishFailedEvent("Unexpected error");
      }
    }

    private static IExam ExecuteBusinessLogic(
      PublishExamCommand command,
      List<StudentRegistrationNumber> existingStudents,
      List<CalculatedStudentGrade> existingGrades)
    {
      Func<StudentRegistrationNumber, bool> checkStudentExists = student => existingStudents.Any(s => s.Equals(student));
      UnvalidatedExam unvalidatedGrades = new(command.InputExamGrades);

      IExam exam = new ValidateExamOperation(checkStudentExists).Transform(unvalidatedGrades);
      exam = new CalculateExamOperation().Transform(exam, existingGrades);
      exam = new PublishExamOperation().Transform(exam);
      return exam;
    }
  }
}
