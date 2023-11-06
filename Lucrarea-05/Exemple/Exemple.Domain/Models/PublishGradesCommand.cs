using System.Collections.Generic;

namespace Exemple.Domain.Models
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
