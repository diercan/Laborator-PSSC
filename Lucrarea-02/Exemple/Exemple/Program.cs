using Examples.Domain.Models;
using System;
using System.Collections.Generic;
using static Examples.Domain.Models.Exam;

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
      UnvalidatedExam unvalidatedGrades = new(listOfGrades);

      //validate grades
      IExam result = ValidateExamGrades(unvalidatedGrades);


      IExam finalResult = result switch
      {
        InvalidatedExam invalidResult => HandleInvalidGrades(invalidResult),
        ValidatedExam validatedResult => PublishExamGrades(validatedResult),

        // we should never reach this case, as the result should always be one validated or invalidated
        PublishedExam publishedResult => throw new InvalidOperationException("Invalid state"),
        // we should never reach this case, as the result should always be one validated or invalidated
        UnvalidatedExam unvalidatedResult => throw new InvalidOperationException("Invalid state"),
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
    private static IExam ValidateExamGrades(UnvalidatedExam unvalidatedGrades) =>
        random.Next(100) > 50 ?
        new InvalidatedExam(new List<UnvalidatedStudentGrade>(), "Random error")
        : new ValidatedExam(new List<ValidatedStudentGrade>());

    private static IExam HandleInvalidGrades(InvalidatedExam invalidExamGrades)
    {
      Console.WriteLine($"Grades could not be published: {invalidExamGrades.Reason}");
      return invalidExamGrades;
    }

    private static IExam PublishExamGrades(ValidatedExam validExamGrades)
    {
      Console.WriteLine("Grades published successfully");
      return new PublishedExam([], DateTime.Now);
    }

    private static string? ReadValue(string prompt)
    {
      Console.Write(prompt);
      return Console.ReadLine();
    }
  }
}
