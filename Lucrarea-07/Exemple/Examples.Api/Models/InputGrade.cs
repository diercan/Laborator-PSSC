namespace Examples.Api.Models;

/// <summary>
/// Forma JSON a unei note de intrare. Contract "prost" în mod intenționat: nu repetă regulile domeniului
/// (interval, format număr matricol) — domeniul este locul unde acestea trăiesc și unde se raportează erorile.
/// </summary>
public sealed record InputGrade(string? RegistrationNumber, decimal? Exam, decimal? Activity);
