using Examples.Domain.Exceptions;
using System.Text.RegularExpressions;

namespace Examples.Domain.Models
{
  public record StudentRegistrationNumber
  {
    public const string Pattern = "^LM[0-9]{5}$";
    public static readonly Regex ValidPattern = new(Pattern);

    public string Value { get; }

    internal StudentRegistrationNumber(string value)
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
