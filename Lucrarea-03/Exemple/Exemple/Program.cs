using Examples.Domain.Commands;
using Examples.Domain.Models;
using Examples.Domain.Workflows;
using System;
using System.Collections.Generic;

namespace Examples
{
  internal class Program
  {
    private static void Main(string[] args)
    {
      UnvalidatedStudentGrade[] listOfGrades = ReadListOfGrades().ToArray();
      PublishGradesCommand command = new(listOfGrades);
      PublishGradeWorkflow workflow = new();
      ExamGradesPublishedEvent.IExamGradesPublishedEvent result = workflow.Execute(command, (registrationNumber) => true);

      string message = result switch
      {
        ExamGradesPublishedEvent.ExamGradesPublishFailedEvent @event => $"Publish failed: {@event.Reason}",
        ExamGradesPublishedEvent.ExamGradesPublishSucceededEvent @event => @event.Csv,
        _ => throw new NotImplementedException()
      };

      Console.WriteLine(message);
    }

    private static List<UnvalidatedStudentGrade> ReadListOfGrades()
    {
      List<UnvalidatedStudentGrade> listOfGrades = [];
      do
      {
        //read registration number and grade and create a list of greads
        string? registrationNumber = ReadValue("Registration Number: ");
        if (string.IsNullOrEmpty(registrationNumber))
        {
          break;
        }

        string? examGrade = ReadValue("Exam Grade: ");
        if (string.IsNullOrEmpty(examGrade))
        {
          break;
        }

        string? activityGrade = ReadValue("Activity Grade: ");
        if (string.IsNullOrEmpty(activityGrade))
        {
          break;
        }

        listOfGrades.Add(new(registrationNumber, examGrade, activityGrade));
      } while (true);
      return listOfGrades;
    }

    private static string? ReadValue(string prompt)
    {
      Console.Write(prompt);
      return Console.ReadLine();
    }
  }
}
