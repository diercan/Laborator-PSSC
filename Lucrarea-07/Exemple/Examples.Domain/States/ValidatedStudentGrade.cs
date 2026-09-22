using Examples.Domain.ValueObjects;

namespace Examples.Domain.States;

/// <summary>
/// Notele unui student după validare: numărul matricol aparține unui student existent, iar ambele note
/// sunt prezente și valabile. Fiind "validat" în acest sens, câmpurile nu mai sunt nullable — starea codifică
/// invariantul, nu doar îl documentează.
/// </summary>
public sealed record ValidatedStudentGrade(StudentRegistrationNumber RegistrationNumber, Grade ExamGrade, Grade ActivityGrade);
