using Examples.Domain.Repositories;
using Examples.Domain.ValueObjects;

namespace Examples.Domain.Tests.Workflows;

/// <summary>Adaptor în memorie pentru <see cref="IStudentsRepository"/>, folosit doar în teste.</summary>
internal sealed class FakeStudentsRepository(params string[] knownRegistrationNumbers) : IStudentsRepository
{
    public Task<IReadOnlySet<StudentRegistrationNumber>> GetExistingAsync(IReadOnlyCollection<string> registrationNumbers, CancellationToken cancellationToken)
    {
        HashSet<StudentRegistrationNumber> known =
        [
            .. knownRegistrationNumbers
                .Where(registrationNumbers.Contains)
                .Select(raw => StudentRegistrationNumber.Create(raw).Match(number => number, _ => throw new InvalidOperationException())),
        ];

        return Task.FromResult<IReadOnlySet<StudentRegistrationNumber>>(known);
    }
}
