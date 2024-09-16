using Examples.Domain.Models;
using System;
using System.Collections.Generic;
using static Examples.Domain.Models.ExamGrades;

namespace Examples.Domain.Operations
{
  internal class ValidateExamGradesOperation
  {
    private readonly Func<StudentRegistrationNumber, bool> checkStudentExists;

    public ValidateExamGradesOperation(Func<StudentRegistrationNumber, bool> checkStudentExists)
    {
      this.checkStudentExists = checkStudentExists;
    }

    public IExamGrades ValidateExamGrades(UnvalidatedExamGrades examGrades)
    {
      List<ValidatedStudentGrade> validatedGrades = [];
      bool isValidList = true;
      string invalidReason = string.Empty;
      foreach (UnvalidatedStudentGrade unvalidatedGrade in examGrades.GradeList)
      {
        if (!Grade.TryParseGrade(unvalidatedGrade.ExamGrade, out Grade examGrade))
        {
          invalidReason = $"Invalid exam grade ({unvalidatedGrade.StudentRegistrationNumber}, {unvalidatedGrade.ExamGrade})";
          isValidList = false;
          break;
        }

        if (!Grade.TryParseGrade(unvalidatedGrade.ActivityGrade, out Grade activityGrade))
        {
          invalidReason = $"Invalid activity grade ({unvalidatedGrade.StudentRegistrationNumber}, {unvalidatedGrade.ActivityGrade})";
          isValidList = false;
          break;
        }

        if (!StudentRegistrationNumber.TryParse(unvalidatedGrade.StudentRegistrationNumber, out StudentRegistrationNumber studentRegistrationNumber)
            && checkStudentExists(studentRegistrationNumber))
        {
          invalidReason = $"Invalid student registration number ({unvalidatedGrade.StudentRegistrationNumber})";
          isValidList = false;
          break;
        }

        ValidatedStudentGrade validGrade = new(studentRegistrationNumber, examGrade, activityGrade);
        validatedGrades.Add(validGrade);
      }

      if (isValidList)
      {
        return new ValidatedExamGrades(validatedGrades);
      }
      else
      {
        return new InvalidatedExamGrades(examGrades.GradeList, invalidReason);
      }
    }
  }
}
