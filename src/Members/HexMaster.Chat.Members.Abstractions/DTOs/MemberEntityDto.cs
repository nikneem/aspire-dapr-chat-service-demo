using Azure;

namespace HexMaster.Chat.Members.Abstractions.DTOs;

public record MemberEntityDto
{
    public string PartitionKey { get; init; } = "member";
    public string RowKey { get; init; } = string.Empty;
    public DateTimeOffset? Timestamp { get; init; }
    public ETag ETag { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public DateTime JoinedAt { get; init; }
    public DateTime LastActivityAt { get; init; }
    public bool IsActive { get; init; }
}
