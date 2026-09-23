using Examples.Domain.Repositories;
using Examples.Domain.ValueObjects;
using Examples.Functional;
using Microsoft.EntityFrameworkCore;

namespace Examples.Data.Repositories;

/// <inheritdoc cref="IStudentsRepository"/>
public sealed class StudentsRepository(GradesContext db) : IStudentsRepository
{
    public async Task<IReadOnlySet<StudentRegistrationNumber>> GetExistingAsync(
        IReadOnlyCollection<string> registrationNumbers, CancellationToken cancellationToken)
    {
        if (registrationNumbers.Count == 0)
        {
            return new HashSet<StudentRegistrationNumber>();
        }

        List<string> found = await db.Students
            .AsNoTracking()
            .Where(student => registrationNumbers.Contains(student.RegistrationNumber))
            .Select(student => student.RegistrationNumber)
            .ToListAsync(cancellationToken);

        // Baza de date proprie e de încredere: dacă un rând nu respectă formatul, e o coruptere a datelor,
        // nu o greșeală a utilizatorului — se aruncă direct, nu se transformă într-o eroare de domeniu.
        return found
            .Select(raw => StudentRegistrationNumber.Create(raw)
                .GetValueOrThrow(_ => new InvalidDataException($"Număr matricol invalid în baza de date: '{raw}'.")))
            .ToHashSet();
    }
}
