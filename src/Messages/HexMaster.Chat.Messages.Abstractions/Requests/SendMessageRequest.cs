namespace HexMaster.Chat.Messages.Abstractions.Requests;

public record SendMessageRequest
{
    public string Content { get; init; } = string.Empty;
    public string SenderId { get; init; } = string.Empty;
}
