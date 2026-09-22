using Examples.Domain.ValueObjects;

namespace Examples.Domain.Repositories;

/// <summary>Port (Wlaschin cap. 9) prin care workflow-ul află care numere matricole aparțin unor studenți existenți.</summary>
public interface IStudentsRepository
{
    /// <summary>Dintre numerele matricole brute primite, întoarce doar cele care aparțin unor studenți existenți.</summary>
    Task<IReadOnlySet<StudentRegistrationNumber>> GetExistingAsync(IReadOnlyCollection<string> registrationNumbers, CancellationToken cancellationToken);
}
