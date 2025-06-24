using Dapr.Client;
using HexMaster.Chat.Messages.Api.Entities;
using HexMaster.Chat.Messages.Api.Repositories;
using HexMaster.Chat.Shared.Constants;
using HexMaster.Chat.Shared.Events;
using HexMaster.Chat.Shared.Models;
using HexMaster.Chat.Shared.Requests;
using System.Text.RegularExpressions;

namespace HexMaster.Chat.Messages.Api.Services;

public interface IMessageService
{
    Task<ChatMessage> SendMessageAsync(SendMessageRequest request);
    Task<IEnumerable<ChatMessage>> GetRecentMessagesAsync(int count = 50);
    Task<IEnumerable<ChatMessage>> GetMessageHistoryAsync(DateTime from, DateTime to);
    Task RemoveExpiredMessagesAsync();
}

public class MessageService : IMessageService
{
    private readonly IMessageRepository _repository;
    private readonly DaprClient _daprClient;
    private readonly IMemberStateService _memberStateService;
    private readonly ILogger<MessageService> _logger;

    public MessageService(
        IMessageRepository repository,
        DaprClient daprClient,
        IMemberStateService memberStateService,
        ILogger<MessageService> logger)
    {
        _repository = repository;
        _daprClient = daprClient;
        _memberStateService = memberStateService;
        _logger = logger;
    }

    public async Task<ChatMessage> SendMessageAsync(SendMessageRequest request)
    {
        // Validate message content
        if (string.IsNullOrWhiteSpace(request.Content))
        {
            throw new ArgumentException("Message content cannot be empty");
        }

        if (request.Content.Length > 1000)
        {
            throw new ArgumentException("Message content too long (max 1000 characters)");
        }

        // Get sender name from state store
        var senderName = await _memberStateService.GetMemberNameAsync(request.SenderId);
        if (string.IsNullOrEmpty(senderName))
        {
            // Fallback to placeholder name if member not found in state store
            senderName = $"User_{request.SenderId[..Math.Min(8, request.SenderId.Length)]}";
            _logger.LogWarning("Member {SenderId} not found in state store, using fallback name", request.SenderId);
        }
        else
        {
            // Update member activity to extend sliding expiration
            await _memberStateService.UpdateMemberActivityAsync(request.SenderId);
        }

        // Basic profanity filtering (in production, use a proper service)
        var cleanContent = FilterProfanity(request.Content);

        var messageId = Guid.NewGuid().ToString();
        var now = DateTime.UtcNow;

        var messageEntity = new MessageEntity
        {
            RowKey = messageId,
            Content = cleanContent,
            SenderId = request.SenderId,
            SenderName = senderName,
            SentAt = now,
            MessageType = (int)MessageType.Text
        };

        await _repository.CreateAsync(messageEntity);

        // Publish message sent event
        var messageSentEvent = new MessageSentEvent
        {
            Id = messageId,
            Content = cleanContent,
            SenderId = request.SenderId,
            SenderName = senderName,
            SentAt = now
        };

        await _daprClient.PublishEventAsync(
            DaprComponents.PubSubName,
            Topics.MessageSent,
            messageSentEvent);

        _logger.LogInformation("Message {MessageId} sent by {SenderId}", messageId, request.SenderId);

        return new ChatMessage
        {
            Id = messageId,
            Content = cleanContent,
            SenderId = request.SenderId,
            SenderName = senderName,
            SentAt = now,
            Type = MessageType.Text
        };
    }

    public async Task<IEnumerable<ChatMessage>> GetRecentMessagesAsync(int count = 50)
    {
        var messageEntities = await _repository.GetRecentMessagesAsync(count);
        return messageEntities.Select(MapToModel);
    }

    public async Task<IEnumerable<ChatMessage>> GetMessageHistoryAsync(DateTime from, DateTime to)
    {
        var messageEntities = await _repository.GetMessagesByDateRangeAsync(from, to);
        return messageEntities.Select(MapToModel);
    }

    public async Task RemoveExpiredMessagesAsync()
    {
        var cutoffTime = DateTime.UtcNow.AddHours(-24);
        var expiredMessages = await _repository.GetExpiredMessagesAsync(cutoffTime);

        var messageIds = expiredMessages.Select(m => m.RowKey).ToList();
        if (messageIds.Any())
        {
            await _repository.DeleteBatchAsync(messageIds);
            _logger.LogInformation("Removed {Count} expired messages", messageIds.Count);
        }
    }

    private static ChatMessage MapToModel(MessageEntity entity)
    {
        return new ChatMessage
        {
            Id = entity.RowKey,
            Content = entity.Content,
            SenderId = entity.SenderId,
            SenderName = entity.SenderName,
            SentAt = entity.SentAt,
            Type = (MessageType)entity.MessageType
        };
    }

    private static string FilterProfanity(string content)
    {
        // Simple profanity filter - in production, use a proper service
        var profanityWords = new[] { "spam", "badword" }; // Add more as needed
        var filtered = content;

        foreach (var word in profanityWords)
        {
            filtered = Regex.Replace(filtered, word, "***", RegexOptions.IgnoreCase);
        }

        return filtered;
    }
}
