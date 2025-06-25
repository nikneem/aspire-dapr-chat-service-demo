namespace HexMaster.Chat.Messages.Abstractions.DTOs;

public record ChatMessageDto
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
