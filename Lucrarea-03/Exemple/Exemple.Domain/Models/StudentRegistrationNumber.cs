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
      if (IsValid(value))
      {
        Value = value;
      }
      else
      {
        throw new InvalidStudentRegistrationNumberException("");
      }
    }

    private static bool IsValid(string stringValue) => ValidPattern.IsMatch(stringValue);

    public override string ToString()
    {
      return Value;
    }

    public static bool TryParse(string stringValue, out StudentRegistrationNumber? registrationNumber)
    {
      bool isValid = false;
      registrationNumber = null;

      if (IsValid(stringValue))
      {
        isValid = true;
        registrationNumber = new(stringValue);
      }

      return isValid;
    }
  }
}
