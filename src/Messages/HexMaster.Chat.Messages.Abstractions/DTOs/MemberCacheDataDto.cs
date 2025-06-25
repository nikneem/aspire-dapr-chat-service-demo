namespace HexMaster.Chat.Messages.Abstractions.DTOs;

public record MemberCacheDataDto
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public DateTime JoinedAt { get; init; }
    public DateTime LastAccessAt { get; init; }
}
