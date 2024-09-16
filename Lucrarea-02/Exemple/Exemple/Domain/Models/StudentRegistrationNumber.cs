using Examples.Domain.Exceptions;
using System.Text.RegularExpressions;

namespace Examples.Domain.Models
{
  public record StudentRegistrationNumber
  {
    private static readonly Regex ValidPattern = new("^LM[0-9]{5}$");

    public string Value { get; }

    private StudentRegistrationNumber(string value)
    {
      if (ValidPattern.IsMatch(value))
      {
        Value = value;
      }
      else
      {
        throw new InvalidStudentRegistrationNumberException("");
      }
    }

    public StudentRegistrationNumber GetStudentRegistrationNumber()
    {
      return new StudentRegistrationNumber(Value);
    }

    public override string ToString()
    {
      return Value;
    }
  }
}
