using Azure;
using Azure.Data.Tables;
using HexMaster.Chat.Messages.Api.Entities;
using HexMaster.Chat.Shared.Constants;

namespace HexMaster.Chat.Messages.Api.Repositories;

public interface IMessageRepository
{
    Task<MessageEntity> CreateAsync(MessageEntity message);
    Task<IEnumerable<MessageEntity>> GetRecentMessagesAsync(int count = 50);
    Task<IEnumerable<MessageEntity>> GetMessagesByDateRangeAsync(DateTime from, DateTime to);
    Task<IEnumerable<MessageEntity>> GetExpiredMessagesAsync(DateTime cutoffTime);
    Task DeleteAsync(string id);
    Task DeleteBatchAsync(IEnumerable<string> ids);
}

public class MessageRepository : IMessageRepository
{
    private readonly TableClient _tableClient;

    public MessageRepository(TableServiceClient tableServiceClient)
    {
        _tableClient = tableServiceClient.GetTableClient(TableNames.Messages);
        _tableClient.CreateIfNotExists();
    }

    public async Task<MessageEntity> CreateAsync(MessageEntity message)
    {
        await _tableClient.AddEntityAsync(message);
        return message;
    }

    public async Task<IEnumerable<MessageEntity>> GetRecentMessagesAsync(int count = 50)
    {
        var entities = new List<MessageEntity>();
        var query = _tableClient.QueryAsync<MessageEntity>(
            maxPerPage: count,
            select: null
        );

        await foreach (var entity in query)
        {
            entities.Add(entity);
            if (entities.Count >= count)
                break;
        }

        return entities.OrderBy(x => x.SentAt).Take(count);
    }

    public async Task<IEnumerable<MessageEntity>> GetMessagesByDateRangeAsync(DateTime from, DateTime to)
    {
        var filter = $"SentAt ge datetime'{from:yyyy-MM-ddTHH:mm:ssZ}' and SentAt le datetime'{to:yyyy-MM-ddTHH:mm:ssZ}'";
        var entities = new List<MessageEntity>();

        await foreach (var entity in _tableClient.QueryAsync<MessageEntity>(filter))
        {
            entities.Add(entity);
        }

        return entities.OrderBy(x => x.SentAt);
    }

    public async Task<IEnumerable<MessageEntity>> GetExpiredMessagesAsync(DateTime cutoffTime)
    {
        var filter = $"SentAt lt datetime'{cutoffTime:yyyy-MM-ddTHH:mm:ssZ}'";
        var entities = new List<MessageEntity>();

        await foreach (var entity in _tableClient.QueryAsync<MessageEntity>(filter))
        {
            entities.Add(entity);
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
}
