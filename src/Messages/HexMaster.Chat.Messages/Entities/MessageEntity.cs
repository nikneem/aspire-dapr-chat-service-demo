using Azure;
using Azure.Data.Tables;

namespace HexMaster.Chat.Messages.Entities;

public class MessageEntity : ITableEntity
{
    public string PartitionKey { get; set; } = "message";
    public string RowKey { get; set; } = string.Empty;
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }

    public string Content { get; set; } = string.Empty;
    public string SenderId { get; set; } = string.Empty;
    public string SenderName { get; set; } = string.Empty;
    public DateTime SentAt { get; set; }
    public int MessageType { get; set; }
}
