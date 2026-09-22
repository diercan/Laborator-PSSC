using System.Diagnostics;
using Examples.Domain.Errors;
using Examples.Domain.States;
using Examples.Domain.ValueObjects;
using Examples.Functional;

// Modulul de validare (Examples.Domain.Operations) vine abia în Lucrarea 3. Aici exersăm doar sistemul de
// tipuri: fiecare notă brută este convertită în obiecte-valoare prin Grade.Parse/StudentRegistrationNumber.Create
// (care întorc Result în loc să arunce excepții, combinate cu Result.Combine ca să adunăm toate erorile unui
// student), iar existența studentului (care în Lucrarea 3 se verifică față de o listă reală) e simulată aleator.
List<UnvalidatedStudentGrade> grades = ReadGrades();
Result<Exam.Validated, IReadOnlyList<ValidationError>> result = SimulateValidation(new Exam.Unvalidated(grades));

string message = result switch
{
    Result<Exam.Validated, IReadOnlyList<ValidationError>>.Ok ok =>
        $"Note publicate cu succes pentru {ok.Value.Grades.Count} studenți.",
    Result<Exam.Validated, IReadOnlyList<ValidationError>>.Error error =>
        $"Publicare eșuată:{Environment.NewLine}{string.Join(Environment.NewLine, error.Value.Select(e => e.ToMessage()))}",
    _ => throw new UnreachableException(),
};

Console.WriteLine(message);

static List<UnvalidatedStudentGrade> ReadGrades()
{
    List<UnvalidatedStudentGrade> grades = [];
    Console.WriteLine("Introduceți numărul matricol și notele fiecărui student. Linie goală pentru a termina.");

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

    return grades;
}

static Result<Exam.Validated, IReadOnlyList<ValidationError>> SimulateValidation(Exam.Unvalidated exam) =>
    exam.Grades.Traverse(ParseGrade).Bind(SimulateStudentsExist);

// Combină cele trei conversii independente și adună toate erorile unui student (nu se oprește la prima).
static Result<ValidatedStudentGrade, IReadOnlyList<ValidationError>> ParseGrade(UnvalidatedStudentGrade grade) =>
    Result.Combine(
        StudentRegistrationNumber.Create(grade.RegistrationNumber)
            .MapError(error => (ValidationError)new ValidationError.InvalidRegistrationNumber(error.Raw)),
        Grade.Parse(grade.ExamGrade)
            .MapError(error => (ValidationError)new ValidationError.InvalidExamGrade(grade.RegistrationNumber, error)),
        Grade.Parse(grade.ActivityGrade)
            .MapError(error => (ValidationError)new ValidationError.InvalidActivityGrade(grade.RegistrationNumber, error)),
        (registrationNumber, examGrade, activityGrade) => new ValidatedStudentGrade(registrationNumber, examGrade, activityGrade));

// Simulează verificarea existenței studentului (regula reală, față de o listă de studenți cunoscuți, vine în Lucrarea 3).
static Result<Exam.Validated, IReadOnlyList<ValidationError>> SimulateStudentsExist(IReadOnlyList<ValidatedStudentGrade> grades)
{
    if (grades.Count > 0 && Random.Shared.Next(2) == 0)
    {
        return new List<ValidationError> { new ValidationError.StudentNotFound(grades[0].RegistrationNumber) };
    }

    return new Exam.Validated(grades);
}
