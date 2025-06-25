using HexMaster.Chat.Messages.Abstractions.DTOs;

namespace HexMaster.Chat.Messages.Abstractions.Interfaces;

public interface IMessageRepository
{
    Task<MessageEntityDto> CreateAsync(MessageEntityDto message);
    Task<IEnumerable<MessageEntityDto>> GetRecentMessagesAsync(int count = 50);
    Task<IEnumerable<MessageEntityDto>> GetMessagesByDateRangeAsync(DateTime from, DateTime to);
    Task<IEnumerable<MessageEntityDto>> GetExpiredMessagesAsync(DateTime cutoffTime);
    Task DeleteAsync(string id);
    Task DeleteBatchAsync(IEnumerable<string> ids);
}
