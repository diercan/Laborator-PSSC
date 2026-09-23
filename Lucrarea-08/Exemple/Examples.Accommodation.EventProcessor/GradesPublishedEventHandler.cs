using Examples.Contracts.Events;
using Examples.Events;
using Microsoft.Extensions.Logging;

namespace Examples.Accommodation.EventProcessor;

/// <summary>
/// Tratează evenimentul de note publicate în contextul cazării. Un exemplu minimal — logează ce a primit;
/// o implementare reală ar actualiza aici, de exemplu, eligibilitatea pentru cazare.
/// </summary>
internal sealed partial class GradesPublishedEventHandler(ILogger<GradesPublishedEventHandler> logger) : IEventHandler<GradesPublishedEvent>
{
    public Task<EventProcessingResult> HandleAsync(GradesPublishedEvent @event, CancellationToken cancellationToken)
    {
        LogReceived(logger, @event.Grades.Count, @event.PublishedAt);
        return Task.FromResult(EventProcessingResult.Completed);
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Received {GradeCount} grades published at {PublishedAt}")]
    private static partial void LogReceived(ILogger logger, int gradeCount, DateTimeOffset publishedAt);
}
