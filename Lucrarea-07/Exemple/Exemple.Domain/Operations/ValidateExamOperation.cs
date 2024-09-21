using Examples.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using static Examples.Domain.Models.Exam;

namespace Examples.Domain.Operations
{
  internal sealed class ValidateExamOperation : ExamOperation
  {
    private readonly Func<StudentRegistrationNumber, bool> checkStudentExists;

    internal ValidateExamOperation(Func<StudentRegistrationNumber, bool> checkStudentExists)
    {
      this.checkStudentExists = checkStudentExists;
    }

    protected override IExam OnUnvalidated(UnvalidatedExam unvalidatedExam)
    {
      (List<ValidatedStudentGrade> validatedGrades, IEnumerable<string> validationErrors) = ValidateListOfGrades(unvalidatedExam);

      if (validationErrors.Any())
      {
        return new InvalidExam(unvalidatedExam.GradeList, validationErrors);
      }
      else
      {
        return new ValidatedExam(validatedGrades);
      }
    }

    private (List<ValidatedStudentGrade>, IEnumerable<string>) ValidateListOfGrades(UnvalidatedExam examGrades)
    {
      List<string> validationErrors = [];
      List<ValidatedStudentGrade> validatedGrades = [];
      foreach (UnvalidatedStudentGrade unvalidatedGrade in examGrades.GradeList)
      {
        ValidatedStudentGrade? validGrade = ValidateGrade(unvalidatedGrade, validationErrors);
        if (validGrade is not null)
        {
          validatedGrades.Add(validGrade);
        }
      }

      return (validatedGrades, validationErrors);
    }

    private ValidatedStudentGrade? ValidateGrade(UnvalidatedStudentGrade unvalidatedGrade, List<string> validationErrors)
    {
      List<string> currentValidationErrors = [];
      Grade? examGrade = ValidateAndParseExamGrade(unvalidatedGrade, currentValidationErrors);
      Grade? activityGrade = ValidateAndParseActivityGrade(unvalidatedGrade, currentValidationErrors);
      StudentRegistrationNumber? studentRegistrationNumber = ValidateAndParseRegistrationNumber(unvalidatedGrade, currentValidationErrors);

      ValidatedStudentGrade? validGrade = null;
      if (!currentValidationErrors.Any())
      {
        validGrade = new(studentRegistrationNumber!, examGrade, activityGrade!);
      }
      else
      {
        validationErrors.AddRange(currentValidationErrors);
      }
      return validGrade;
    }

    private static Grade? ValidateAndParseExamGrade(UnvalidatedStudentGrade unvalidatedGrade, List<string> validationErrors)
    {
      if (!Grade.TryParseGrade(unvalidatedGrade.ExamGrade, out Grade? examGrade))
      {
        validationErrors.Add($"Invalid exam grade ({unvalidatedGrade.StudentRegistrationNumber}, {unvalidatedGrade.ExamGrade})");
      }

      return examGrade;
    }

    private static Grade? ValidateAndParseActivityGrade(UnvalidatedStudentGrade unvalidatedGrade, List<string> validationErrors)
    {
      Grade? activityGrade;
      if (!Grade.TryParseGrade(unvalidatedGrade.ActivityGrade, out activityGrade))
      {
        validationErrors.Add($"Invalid activity grade ({unvalidatedGrade.StudentRegistrationNumber}, {unvalidatedGrade.ActivityGrade})");
      }

      return activityGrade;
    }

    private StudentRegistrationNumber? ValidateAndParseRegistrationNumber(UnvalidatedStudentGrade unvalidatedGrade, List<string> validationErrors)
    {
      StudentRegistrationNumber? studentRegistrationNumber;
      if (!StudentRegistrationNumber.TryParse(unvalidatedGrade.StudentRegistrationNumber, out studentRegistrationNumber))
      {
        validationErrors.Add($"Invalid student registration number ({unvalidatedGrade.StudentRegistrationNumber})");
      }
      else if (!checkStudentExists(studentRegistrationNumber!))
      {
        validationErrors.Add($"Student not found ({unvalidatedGrade.StudentRegistrationNumber})");
      }

      return studentRegistrationNumber;
    }
  }
}
