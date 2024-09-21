namespace Example.Dto.Models
{
  public record StudentGradeDto
  {
    public string StudentRegistrationNumber { get; init; }
    public decimal? ActivityGrade { get; init; }
    public decimal? ExamGrade { get; init; }
    public decimal? FinalGrade { get; init; }
  }
}
