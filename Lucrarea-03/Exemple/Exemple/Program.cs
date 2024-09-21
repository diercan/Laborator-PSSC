using Examples.Domain.Models;
using Examples.Domain.Workflows;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Examples
{
  internal class Program
  {
    private static void Main(string[] args)
    {
      UnvalidatedStudentGrade[] listOfGrades = ReadListOfGrades().ToArray();
      PublishExamCommand command = new(listOfGrades);
      PublishExamWorkflow workflow = new();
      ExamPublishedEvent.IExamPublishedEvent result = workflow.Execute(command, CheckStudentExists);

      string message = result switch
      {
        ExamPublishedEvent.ExamPublishSucceededEvent @event => @event.Csv,
        ExamPublishedEvent.ExamPublishFailedEvent @event => $"Publish failed: \r\n{string.Join("\r\n", @event.Reasons)}",
        _ => throw new NotImplementedException()
      };

      Console.WriteLine();
      Console.WriteLine("============================");
      Console.WriteLine("Catalog Note:");
      Console.WriteLine("============================");
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

    private static bool CheckStudentExists(StudentRegistrationNumber registrationNumber) =>
      existingStudents.Contains(registrationNumber.Value);

    private static readonly IEnumerable<string> existingStudents = ["LM12345", "LM54321", "LM67890", "LM98765"];
  }
}
