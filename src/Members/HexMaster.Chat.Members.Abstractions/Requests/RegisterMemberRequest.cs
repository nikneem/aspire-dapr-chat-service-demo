namespace HexMaster.Chat.Members.Abstractions.Requests;

public record RegisterMemberRequest
{
    public string Name { get; init; } = string.Empty;
}
