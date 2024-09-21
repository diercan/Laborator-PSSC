using System;

namespace Examples.Domain.Exceptions
{
  public class InvalidGradeException : Exception
  {
    public InvalidGradeException()
    {
    }

    public InvalidGradeException(string? message) : base(message)
    {
    }

    public InvalidGradeException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
  }
}