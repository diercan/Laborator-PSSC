using Examples.Domain.Models;
using System.Collections.Generic;

namespace Examples.Domain.Commands
{
  public record PublishGradesCommand
  {
    public PublishGradesCommand(IReadOnlyCollection<UnvalidatedStudentGrade> inputExamGrades)
    {
      InputExamGrades = inputExamGrades;
    }

    public IReadOnlyCollection<UnvalidatedStudentGrade> InputExamGrades { get; }
  }
}
