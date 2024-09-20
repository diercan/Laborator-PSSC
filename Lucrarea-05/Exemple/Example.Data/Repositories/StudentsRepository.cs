using Examples.Domain.Models;
using Examples.Domain.Repositories;
using LanguageExt;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Example.Data.Repositories
{
  public class StudentsRepository : IStudentsRepository
  {
    private readonly GradesContext gradesContext;

    public StudentsRepository(GradesContext gradesContext)
    {
      this.gradesContext = gradesContext;
    }

    public async Task<List<StudentRegistrationNumber>> GetExistingStudentsAsync(IEnumerable<string> studentsToCheck)
    {
      List<Models.StudentDto> students = await gradesContext.Students
              .Where(student => studentsToCheck.Contains(student.RegistrationNumber))
              .AsNoTracking()
              .ToListAsync();
      return students.Select(student => new StudentRegistrationNumber(student.RegistrationNumber))
                     .ToList();
    }
  }
}
