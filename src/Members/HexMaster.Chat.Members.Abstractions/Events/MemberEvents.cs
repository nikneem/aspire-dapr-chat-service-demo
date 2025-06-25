namespace HexMaster.Chat.Members.Abstractions.Events;

public record MemberJoinedEvent
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public DateTime JoinedAt { get; init; }
}

public record MemberLeftEvent
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public DateTime LeftAt { get; init; }
}
