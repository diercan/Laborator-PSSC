﻿using Examples.Domain.Exceptions;
using System;

namespace Examples.Domain.Models
{
  public record Grade
  {

    public decimal Value { get; }

    private Grade()
    {
      Value = 0;
    }

    public Grade(decimal value)
    {
      if (IsValid(value))
      {
        Value = value;
      }
      else
      {
        throw new InvalidGradeException($"{value:0.##} is an invalid grade value.");
      }
    }

    public static Grade operator +(Grade a, Grade b) => new((a.Value + b.Value) / 2m);


    public Grade Round()
    {
      decimal roundedValue = Math.Round(Value);
      return new Grade(roundedValue);
    }

    public override string ToString() => $"{Value:0.##}";

    public static bool TryParseGrade(string? gradeString, out Grade? grade)
    {
      bool isValid = false;
      grade = null;
      if (decimal.TryParse(gradeString, out decimal numericGrade))
      {
        if (IsValid(numericGrade))
        {
          isValid = true;
          grade = new(numericGrade);
        }
      }

      return isValid;
    }

    private static bool IsValid(decimal numericGrade) => numericGrade > 0 && numericGrade <= 10;
  }
}
