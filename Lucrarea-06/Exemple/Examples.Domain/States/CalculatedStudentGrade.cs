using Examples.Domain.ValueObjects;
using Examples.Functional;

namespace Examples.Domain.States;

/// <summary>
/// Notele unui student după calculul notei finale. Nota finală poate lipsi legitim (dacă una dintre note
/// este sub pragul de promovare), deci este reprezentată prin <see cref="Option{T}"/>, nu prin <c>Grade?</c>:
/// apelantul este obligat să trateze explicit absența, nu doar să o ignore cu <c>!</c>.
/// </summary>
public sealed record CalculatedStudentGrade(
    StudentRegistrationNumber RegistrationNumber,
    Grade ExamGrade,
    Grade ActivityGrade,
    Option<Grade> FinalGrade);
