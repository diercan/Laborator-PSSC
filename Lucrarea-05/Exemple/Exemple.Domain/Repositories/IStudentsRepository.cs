using Exemple.Domain.Models;
using LanguageExt;
using System.Collections.Generic;

namespace Exemple.Domain.Repositories
{
    public interface IStudentsRepository
    {
        TryAsync<List<StudentRegistrationNumber>> TryGetExistingStudents(IEnumerable<string> studentsToCheck);
    }
}
