namespace Examples.Data.Queries;

/// <summary>
/// O linie din catalogul curent, pentru citire directă (fără validarea domeniului): baza de date permite
/// note lipsă, ceea ce <c>ValidatedStudentGrade</c> din domeniu nu poate reprezenta.
/// </summary>
public sealed record StudentGradeRow(string RegistrationNumber, string Name, decimal? Exam, decimal? Activity, decimal? Final);
