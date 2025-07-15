namespace HexMaster.Chat.Shared.Requests;

public record RegisterMemberRequest
{
    public string Name { get; init; } = string.Empty;
}

public record SendMessageRequest
{
    public string Content { get; init; } = string.Empty;
    public string SenderId { get; init; } = string.Empty;
}
