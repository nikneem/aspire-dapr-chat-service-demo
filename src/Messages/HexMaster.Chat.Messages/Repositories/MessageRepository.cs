using Azure;
using Azure.Data.Tables;
using HexMaster.Chat.Messages.Entities;
using HexMaster.Chat.Messages.Abstractions.DTOs;
using HexMaster.Chat.Messages.Abstractions.Interfaces;
using HexMaster.Chat.Shared.Constants;

namespace HexMaster.Chat.Messages.Repositories;

public class MessageRepository : IMessageRepository
{
    private readonly TableClient _tableClient;

    public MessageRepository(TableServiceClient tableServiceClient)
    {
        _tableClient = tableServiceClient.GetTableClient(TableNames.Messages);
        _tableClient.CreateIfNotExists();
    }

    public async Task<MessageEntityDto> CreateAsync(MessageEntityDto message)
    {
        var entity = MapToEntity(message);
        await _tableClient.AddEntityAsync(entity);
        return MapToDto(entity);
    }

    public async Task<IEnumerable<MessageEntityDto>> GetRecentMessagesAsync(int count = 50)
    {
        var entities = new List<MessageEntityDto>();
        var query = _tableClient.QueryAsync<MessageEntity>(
            maxPerPage: count,
            select: null
        );

        await foreach (var entity in query)
        {
            entities.Add(MapToDto(entity));
            if (entities.Count >= count)
                break;
        }

        var latestMessages = entities.OrderByDescending(x => x.SentAt).Take(count);
        return latestMessages.OrderBy(x => x.SentAt);
    }

    public async Task<IEnumerable<MessageEntityDto>> GetMessagesByDateRangeAsync(DateTime from, DateTime to)
    {
        var filter = $"SentAt ge datetime'{from:yyyy-MM-ddTHH:mm:ssZ}' and SentAt le datetime'{to:yyyy-MM-ddTHH:mm:ssZ}'";
        var entities = new List<MessageEntityDto>();

        await foreach (var entity in _tableClient.QueryAsync<MessageEntity>(filter))
        {
            entities.Add(MapToDto(entity));
        }

        return entities.OrderBy(x => x.SentAt);
    }

    public async Task<IEnumerable<MessageEntityDto>> GetExpiredMessagesAsync(DateTime cutoffTime)
    {
        var filter = $"SentAt lt datetime'{cutoffTime:yyyy-MM-ddTHH:mm:ssZ}'";
        var entities = new List<MessageEntityDto>();

        await foreach (var entity in _tableClient.QueryAsync<MessageEntity>(filter))
        {
            entities.Add(MapToDto(entity));
        }

        return entities;
    }

    public async Task DeleteAsync(string id)
    {
        await _tableClient.DeleteEntityAsync("message", id);
    }

    public async Task DeleteBatchAsync(IEnumerable<string> ids)
    {
        const int batchSize = 100;
        var batches = ids.Chunk(batchSize);

        foreach (var batch in batches)
        {
            var deleteActions = batch.Select(id => new TableTransactionAction(
                TableTransactionActionType.Delete,
                new MessageEntity { PartitionKey = "message", RowKey = id, ETag = ETag.All }
            ));

            try
            {
                await _tableClient.SubmitTransactionAsync(deleteActions);
            }
            catch (RequestFailedException ex)
            {
                // Log error but continue with next batch
                // In production, you might want more sophisticated error handling
                Console.WriteLine($"Failed to delete batch: {ex.Message}");
            }
        }
    }

    private static MessageEntity MapToEntity(MessageEntityDto dto)
    {
        return new MessageEntity
        {
            PartitionKey = dto.PartitionKey,
            RowKey = dto.RowKey,
            Timestamp = dto.Timestamp,
            ETag = dto.ETag,
            Content = dto.Content,
            SenderId = dto.SenderId,
            SenderName = dto.SenderName,
            SentAt = dto.SentAt,
            MessageType = (int)dto.Type
        };
    }

    private static MessageEntityDto MapToDto(MessageEntity entity)
    {
        return new MessageEntityDto
        {
            PartitionKey = entity.PartitionKey,
            RowKey = entity.RowKey,
            Timestamp = entity.Timestamp,
            ETag = entity.ETag,
            Content = entity.Content,
            SenderId = entity.SenderId,
            SenderName = entity.SenderName,
            SentAt = entity.SentAt,
            Type = (MessageType)entity.MessageType
        };
    }
}
