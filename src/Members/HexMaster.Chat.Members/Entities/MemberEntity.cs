using Azure;
using Azure.Data.Tables;

namespace HexMaster.Chat.Members.Entities;

public class MemberEntity : ITableEntity
{
    public string PartitionKey { get; set; } = "member";
    public string RowKey { get; set; } = string.Empty;
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }

    public string Name { get; set; } = string.Empty;
    public DateTime JoinedAt { get; set; }
    public DateTime LastActivityAt { get; set; }
    public bool IsActive { get; set; }
}
