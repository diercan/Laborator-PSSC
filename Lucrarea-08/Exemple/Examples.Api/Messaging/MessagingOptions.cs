using System.ComponentModel.DataAnnotations;

namespace Examples.Api.Messaging;

/// <summary>Opțiuni pentru evenimentele publicate de acest API, legate din secțiunea <see cref="SectionName"/> din configurație.</summary>
public sealed class MessagingOptions
{
    public const string SectionName = "Messaging";

    /// <summary>Topicul pe care se publică evenimentul de note publicate.</summary>
    [Required(AllowEmptyStrings = false)]
    public string GradesTopic { get; set; } = string.Empty;
}
