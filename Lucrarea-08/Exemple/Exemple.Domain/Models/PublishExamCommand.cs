using System.Collections.Generic;

namespace Examples.Domain.Models
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
