using Examples.Domain.Errors;
using Examples.Domain.States;
using Examples.Domain.ValueObjects;
using Examples.Functional;

namespace Examples.Domain.Operations;

/// <summary>
/// Validarea examenului: transformă notele brute (<see cref="Exam.Unvalidated"/>) în note validate
/// (<see cref="Exam.Validated"/>), sau întoarce <b>toate</b> erorile găsite (nu doar prima) — stilul de
/// validare "aplicativ" din Wlaschin, cap. 10.
/// </summary>
public static class ExamValidation
{
    extension(Exam.Unvalidated exam)
    {
        /// <summary>
        /// Validează fiecare notă din <paramref name="exam"/> față de mulțimea studenților cunoscuți.
        /// Funcția este pură: nu citește nimic din afară, primește tot ce are nevoie ca parametru.
        /// </summary>
        public Result<Exam.Validated, IReadOnlyList<ValidationError>> Validate(IReadOnlySet<StudentRegistrationNumber> knownStudents) =>
            exam.Grades
                .Traverse(grade => ValidateGrade(grade, knownStudents))
                .Bind(RejectDuplicates)
                .Map(grades => new Exam.Validated(grades));
    }

    private static Result<ValidatedStudentGrade, IReadOnlyList<ValidationError>> ValidateGrade(
        UnvalidatedStudentGrade grade, IReadOnlySet<StudentRegistrationNumber> knownStudents) =>
        Result.Combine(
            ValidateRegistrationNumber(grade.RegistrationNumber, knownStudents),
            // Rezultatul lambdei e trecut explicit la ValidationError (tipul de bază) ca MapError să nu
            // deducă tipul concret derivat (InvalidExamGrade) — vezi nota din PublishExamWorkflow despre
            // argumentele de tip explicite la membri de extensie (C# 14).
            Grade.Parse(grade.ExamGrade).MapError(error => (ValidationError)new ValidationError.InvalidExamGrade(grade.RegistrationNumber, error)),
            Grade.Parse(grade.ActivityGrade).MapError(error => (ValidationError)new ValidationError.InvalidActivityGrade(grade.RegistrationNumber, error)),
            (registrationNumber, examGrade, activityGrade) => new ValidatedStudentGrade(registrationNumber, examGrade, activityGrade));

    private static Result<StudentRegistrationNumber, ValidationError> ValidateRegistrationNumber(
        string? raw, IReadOnlySet<StudentRegistrationNumber> knownStudents) =>
        StudentRegistrationNumber.Create(raw)
            .MapError(error => (ValidationError)new ValidationError.InvalidRegistrationNumber(error.Raw))
            .Ensure(knownStudents.Contains, number => new ValidationError.StudentNotFound(number));

    private static Result<IReadOnlyList<ValidatedStudentGrade>, IReadOnlyList<ValidationError>> RejectDuplicates(
        IReadOnlyList<ValidatedStudentGrade> grades)
    {
        IReadOnlyList<ValidationError> duplicates =
        [
            .. grades
                .GroupBy(grade => grade.RegistrationNumber)
                .Where(group => group.Count() > 1)
                .Select(group => new ValidationError.DuplicateRegistrationNumber(group.Key)),
        ];

        return duplicates.Count == 0
            ? new Result<IReadOnlyList<ValidatedStudentGrade>, IReadOnlyList<ValidationError>>.Ok(grades)
            : new Result<IReadOnlyList<ValidatedStudentGrade>, IReadOnlyList<ValidationError>>.Error(duplicates);
    }
}
