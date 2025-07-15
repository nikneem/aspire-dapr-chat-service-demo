namespace HexMaster.Chat.Members.Abstractions.DTOs;

public record ChatMemberDto
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public DateTime JoinedAt { get; init; }
    public DateTime LastActivityAt { get; init; }
    public bool IsActive { get; init; }
}
