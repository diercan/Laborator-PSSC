namespace Examples.Domain.States;

/// <summary>
/// Notele unui student exact cum au fost primite (text brut, posibil lipsă sau greșit), înainte de orice
/// validare. Wlaschin, cap. 8: starea "unvalidated" păstrează datele primite ca atare, fără să presupună nimic despre ele.
/// </summary>
public sealed record UnvalidatedStudentGrade(string? RegistrationNumber, string? ExamGrade, string? ActivityGrade);
