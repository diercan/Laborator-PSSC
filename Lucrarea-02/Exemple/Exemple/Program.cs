using Examples.Domain.Models;
using System;
using System.Collections.Generic;
using static Examples.Domain.Models.ExamGrades;

namespace Examples
{
  internal class Program
  {
    private static readonly Random random = new();

    private static void Main(string[] args)
    {
      //read registration number and grade and create a list of grades
      UnvalidatedStudentGrade[] listOfGrades = ReadListOfGrades().ToArray();

      //map user input to unvalidated grades
      UnvalidatedExamGrades unvalidatedGrades = new(listOfGrades);

      //validate grades
      IExamGrades result = ValidateExamGrades(unvalidatedGrades);


      IExamGrades finalResult = result switch
      {
        InvalidatedExamGrades invalidResult => HandleInvalidGrades(invalidResult),
        ValidatedExamGrades validatedResult => PublishExamGrades(validatedResult),

        // we should never reach this case, as the result should always be one validated or invalidated
        PublishedExamGrades publishedResult => throw new InvalidOperationException("Invalid state"),
        // we should never reach this case, as the result should always be one validated or invalidated
        UnvalidatedExamGrades unvalidatedResult => throw new InvalidOperationException("Invalid state"),
        //this is a catch all case, if the switch expression does not match any of the previous cases
        _ => throw new InvalidOperationException("Invalid state")
      };
    }

    private static List<UnvalidatedStudentGrade> ReadListOfGrades()
    {
      List<UnvalidatedStudentGrade> listOfGrades = [];
      Console.WriteLine("Enter the registration number and grade for each student. Press enter without typing anything to finish.");
      do
      {
        //read registration number and grade and create a list of greads
        string? registrationNumber = ReadValue("Registration Number: ");
        if (string.IsNullOrEmpty(registrationNumber))
        {
          break;
        }

        string? grade = ReadValue("Grade: ");
        if (string.IsNullOrEmpty(grade))
        {
          break;
        }

        listOfGrades.Add(new(registrationNumber, grade));
      } while (true);
      return listOfGrades;
    }

    //simulate grades validation by returning a random result
    private static IExamGrades ValidateExamGrades(UnvalidatedExamGrades unvalidatedGrades) =>
        random.Next(100) > 50 ?
        new InvalidatedExamGrades(new List<UnvalidatedStudentGrade>(), "Random error")
        : new ValidatedExamGrades(new List<ValidatedStudentGrade>());

    private static IExamGrades HandleInvalidGrades(InvalidatedExamGrades invalidExamGrades)
    {
      Console.WriteLine($"Grades could not be published: {invalidExamGrades.Reason}");
      return invalidExamGrades;
    }

    private static IExamGrades PublishExamGrades(ValidatedExamGrades validExamGrades)
    {
      Console.WriteLine("Grades published successfully");
      return new PublishedExamGrades([], DateTime.Now);
    }

    private static string? ReadValue(string prompt)
    {
      Console.Write(prompt);
      return Console.ReadLine();
    }
  }
}
