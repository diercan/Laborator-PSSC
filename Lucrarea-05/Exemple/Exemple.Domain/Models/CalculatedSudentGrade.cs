namespace Examples.Domain.Models
{
  public record CalculatedStudentGrade(StudentRegistrationNumber StudentRegistrationNumber, Grade ExamGrade, Grade ActivityGrade, Grade FinalGrade)
  {
    public int GradeId { get; set; }
    public bool IsUpdated { get; set; }
  }
}
