using System.ComponentModel.DataAnnotations;

namespace Examples.Accommodation.EventProcessor;

/// <summary>Ce topic și ce subscripție ascultă acest worker, legate din secțiunea <see cref="SectionName"/> din configurație.</summary>
public sealed class EventProcessorOptions
{
    public const string SectionName = "EventProcessor";

    [Required(AllowEmptyStrings = false)]
    public string TopicName { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false)]
    public string SubscriptionName { get; set; } = string.Empty;
}
