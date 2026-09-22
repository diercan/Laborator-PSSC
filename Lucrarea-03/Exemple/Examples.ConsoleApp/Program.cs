using Examples.Domain.Commands;
using Examples.Domain.Errors;
using Examples.Domain.States;
using Examples.Domain.ValueObjects;
using Examples.Domain.Workflows;
using Examples.Functional;

// La acest stadiu nu există încă o bază de date (vine în Lucrarea 5): studenții "existenți" sunt o listă
// fixă în memorie, iar workflow-ul rulează sincron, folosind doar nucleul pur PublishExamWorkflow.Publish
// (fără porturi, fără I/O) — exact pipeline-ul validare -> calcul -> publicare din Lucrarea 3.
HashSet<StudentRegistrationNumber> knownStudents =
[
    .. KnownRegistrationNumbers()
        .Select(raw => StudentRegistrationNumber.Create(raw).Match(
            number => number,
            _ => throw new InvalidOperationException($"Număr matricol hard-codat invalid: '{raw}'."))),
];

List<UnvalidatedStudentGrade> grades = [];
Console.WriteLine("Introduceți notele (număr matricol, notă examen, notă activitate). Linie goală pentru a termina.");

while (true)
{
    Console.Write("Număr matricol: ");
    string? registrationNumber = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(registrationNumber))
    {
        break;
    }

    Console.Write("Notă examen: ");
    string? examGrade = Console.ReadLine();

    Console.Write("Notă activitate: ");
    string? activityGrade = Console.ReadLine();

    grades.Add(new UnvalidatedStudentGrade(registrationNumber, examGrade, activityGrade));
}

PublishExamCommand command = new(grades);
Result<Exam.Published, PublishExamError> result = PublishExamWorkflow.Publish(command, knownStudents, DateTimeOffset.UtcNow);

Console.WriteLine(result.Match(
    published => published.Csv,
    error => $"Publicare eșuată:{Environment.NewLine}{error}"));

// La acest stadiu nu există încă o bază de date: lista de studenți existenți e fixă, în memorie.
static string[] KnownRegistrationNumbers() => ["LM12345", "LM54321", "LM67890", "LM98765"];
