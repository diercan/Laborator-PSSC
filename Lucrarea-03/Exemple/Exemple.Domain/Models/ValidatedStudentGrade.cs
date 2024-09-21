namespace Examples.Domain.Models
{
  public record ValidatedStudentGrade(StudentRegistrationNumber StudentRegistrationNumber, Grade? ExamGrade, Grade? ActivityGrade);
}
