using Examples.Domain.Commands;
using Examples.Domain.Models;
using Examples.Domain.Operations;
using System;
using static Examples.Domain.Models.Exam;
using static Examples.Domain.Models.ExamPublishedEvent;

namespace Examples.Domain.Workflows
{
  public class PublishGradeWorkflow
  {
    public IExamPublishedEvent Execute(PublishExamCommand command, Func<StudentRegistrationNumber, bool> checkStudentExists)
    {
      UnvalidatedExam unvalidatedGrades = new(command.InputExamGrades);

      IExam exam = new ValidateExamOperation(checkStudentExists).Transform(unvalidatedGrades);
      exam = new CalculateExamOperation().Transform(exam);
      exam = new PublishExamOperation().Transform(exam);

      return exam.ToEvent();
    }
  }
}
