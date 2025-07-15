namespace HexMaster.Chat.Shared.Events;

public record MemberJoinedEvent
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public DateTime JoinedAt { get; init; }
}

public record MemberLeftEvent
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public DateTime LeftAt { get; init; }
}

public record MessageSentEvent
{
    public string Id { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public string SenderId { get; init; } = string.Empty;
    public string SenderName { get; init; } = string.Empty;
    public DateTime SentAt { get; init; }
}
