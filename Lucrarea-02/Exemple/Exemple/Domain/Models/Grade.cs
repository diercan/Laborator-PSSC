using Examples.Domain.Exceptions;
using System;

namespace Examples.Domain.Models
{
  public record Grade
  {
    public decimal Value { get; }

    private Grade(decimal value)
    {
      if (value > 0 && value <= 10)
      {
        Value = value;
      }
      else
      {
        throw new InvalidGradeException($"{value:0.##} is an invalid grade value.");
      }
    }

    public Grade Round()
    {
      decimal roundedValue = Math.Round(Value);
      return new Grade(roundedValue);
    }

    public override string ToString()
    {
      return $"{Value:0.##}";
    }
  }
}
