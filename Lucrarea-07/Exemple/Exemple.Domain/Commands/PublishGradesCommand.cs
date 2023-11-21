using System.Collections.Generic;
using Exemple.Domain.Models;

namespace Exemple.Domain.Commands
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
