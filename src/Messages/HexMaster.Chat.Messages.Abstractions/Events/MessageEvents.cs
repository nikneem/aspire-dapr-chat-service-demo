namespace HexMaster.Chat.Messages.Abstractions.Events;

public record MessageSentEvent
{
    public string Id { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public string SenderId { get; init; } = string.Empty;
    public string SenderName { get; init; } = string.Empty;
    public DateTime SentAt { get; init; }
}
