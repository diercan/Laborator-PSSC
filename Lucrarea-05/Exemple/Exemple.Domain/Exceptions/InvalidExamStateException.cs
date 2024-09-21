using System;

namespace Examples.Domain.Exceptions
{
  public class InvalidExamStateException : Exception
  {
    public InvalidExamStateException()
    {
    }

    public InvalidExamStateException(string state) : base($"State {state} not implemented")
    {
    }

    public InvalidExamStateException(string state, Exception innerException) : base($"State {state} not implemented", innerException)
    {
    }
  }
}