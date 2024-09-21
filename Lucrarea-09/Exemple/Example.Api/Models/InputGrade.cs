using Examples.Domain.Models;
using System.ComponentModel.DataAnnotations;

namespace Example.Api.Models
{
  public class InputGrade
  {
    [Required]
    [RegularExpression(StudentRegistrationNumber.Pattern)]
    public string RegistrationNumber { get; set; } = string.Empty;

    [Required]
    [Range(1, 10)]
    public decimal Exam { get; set; }

    [Required]
    [Range(1, 10)]
    public decimal Activity { get; set; }
  }
}
