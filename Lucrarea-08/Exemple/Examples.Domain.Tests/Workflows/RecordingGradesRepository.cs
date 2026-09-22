using Examples.Domain.Repositories;
using Examples.Domain.States;

namespace Examples.Domain.Tests.Workflows;

/// <summary>Adaptor pentru <see cref="IGradesRepository"/> care reține ce anume i s-a cerut să salveze, pentru verificare în teste.</summary>
internal sealed class RecordingGradesRepository : IGradesRepository
{
    private readonly List<Exam.Published> saved = [];

    public IReadOnlyList<Exam.Published> Saved => saved;

    public Task SaveAsync(Exam.Published exam, CancellationToken cancellationToken)
    {
        saved.Add(exam);
        return Task.CompletedTask;
    }
}
