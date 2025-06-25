using Azure;

namespace HexMaster.Chat.Messages.Abstractions.DTOs;

public record MessageEntityDto
{
    public string PartitionKey { get; init; } = "message";
    public string RowKey { get; init; } = string.Empty;
    public DateTimeOffset? Timestamp { get; init; }
    public ETag ETag { get; init; }
    public string Content { get; init; } = string.Empty;
    public string SenderId { get; init; } = string.Empty;
    public string SenderName { get; init; } = string.Empty;
    public DateTime SentAt { get; init; }
    public MessageType Type { get; init; } = MessageType.Text;
}
