using HexMaster.Chat.Messages.Abstractions.DTOs;
using HexMaster.Chat.Messages.Abstractions.Requests;

namespace HexMaster.Chat.Messages.Abstractions.Interfaces;

public interface IMessageService
{
    Task<ChatMessageDto> SendMessageAsync(SendMessageRequest request);
    Task<IEnumerable<ChatMessageDto>> GetRecentMessagesAsync(int count = 50);
    Task<IEnumerable<ChatMessageDto>> GetMessageHistoryAsync(DateTime from, DateTime to);
    Task RemoveExpiredMessagesAsync();
}
