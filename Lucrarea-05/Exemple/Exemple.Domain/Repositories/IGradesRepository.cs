using Examples.Domain.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using static Examples.Domain.Models.ExamGrades;

namespace Exemple.Domain.Repositories
{
  public interface IGradesRepository
  {
    Task<List<CalculatedStudentGrade>> GetExistingGradesAsync();

    Task SaveGradesAsync(PublishedExamGrades grades);
  }
}
