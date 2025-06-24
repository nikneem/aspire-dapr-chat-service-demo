namespace HexMaster.Chat.Shared.Models;

public record ChatMember
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public DateTime JoinedAt { get; init; }
    public DateTime LastActivityAt { get; init; }
    public bool IsActive { get; init; }
}

public record ChatMessage
{
    public string Id { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public string SenderId { get; init; } = string.Empty;
    public string SenderName { get; init; } = string.Empty;
    public DateTime SentAt { get; init; }
    public MessageType Type { get; init; } = MessageType.Text;
}

public enum MessageType
{
    Text,
    System,
    Emoji
}
