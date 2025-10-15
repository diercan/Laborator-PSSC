using Examples.Domain.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Examples.Domain.Repositories
{
  public interface IStudentsRepository
  {
    Task<List<StudentRegistrationNumber>> GetExistingStudentsAsync(IEnumerable<string> studentsToCheck);
  }
}
