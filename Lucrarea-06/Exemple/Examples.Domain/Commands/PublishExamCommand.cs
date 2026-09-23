using Examples.Domain.States;

namespace Examples.Domain.Commands;

/// <summary>Comanda care pornește workflow-ul de publicare a notelor unui examen (Wlaschin cap. 3: comenzile sunt intenția utilizatorului).</summary>
public sealed record PublishExamCommand(IReadOnlyList<UnvalidatedStudentGrade> Grades)
{
    /// <summary>Numerele matricole distincte, brute, cerute de la marginea impură pentru a afla care studenți există.</summary>
    public IReadOnlyCollection<string> RegistrationNumbers =>
        [.. Grades.Select(grade => grade.RegistrationNumber).OfType<string>().Distinct()];
}
