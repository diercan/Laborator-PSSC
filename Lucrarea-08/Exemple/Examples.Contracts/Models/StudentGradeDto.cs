namespace Examples.Contracts.Models;

/// <summary>Notele unui student, așa cum circulă pe fir (JSON) între contexte — fără reguli de validare, doar date.</summary>
public sealed record StudentGradeDto(string RegistrationNumber, decimal? ExamGrade, decimal? ActivityGrade, decimal? FinalGrade);
