namespace Exemple.Domain.Models
{
	public record CalculatedSudentGrade(StudentRegistrationNumber StudentRegistrationNumber, Grade ExamGrade, Grade ActivityGrade, Grade FinalGrade);
}
