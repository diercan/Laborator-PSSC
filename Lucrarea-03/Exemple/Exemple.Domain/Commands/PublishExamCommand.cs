using Examples.Domain.Models;
using System.Collections.Generic;

namespace Examples.Domain.Commands
{
  public record PublishExamCommand
  {
    public PublishExamCommand(IReadOnlyCollection<UnvalidatedStudentGrade> inputExamGrades)
    {
      InputExamGrades = inputExamGrades;
    }

    public IReadOnlyCollection<UnvalidatedStudentGrade> InputExamGrades { get; }
  }
}
