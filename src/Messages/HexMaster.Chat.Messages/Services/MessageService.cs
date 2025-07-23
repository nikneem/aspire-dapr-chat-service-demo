using Dapr.Client;
using HexMaster.Chat.Messages.Abstractions.DTOs;
using HexMaster.Chat.Messages.Abstractions.Interfaces;
using HexMaster.Chat.Messages.Abstractions.Requests;
using HexMaster.Chat.Shared.Constants;
using HexMaster.Chat.Shared.Events;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace HexMaster.Chat.Messages.Services;

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

    public async Task<ChatMessageDto> SendMessageAsync(SendMessageRequest request)
    {
        // Validate sender ID
        if (string.IsNullOrWhiteSpace(request.SenderId))
        {
            throw new ArgumentException("Sender ID cannot be empty");
        }

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

        var messageDto = new MessageEntityDto
        {
            RowKey = messageId,
            Content = cleanContent,
            SenderId = request.SenderId,
            SenderName = senderName,
            SentAt = now,
            Type = MessageType.Text
        };

        await _repository.CreateAsync(messageDto);

        // Publish message sent event
        var messageSentEvent = new MessageSentEvent
        {
            Id = messageId,
            Content = cleanContent,
            SenderId = request.SenderId,
            SenderName = senderName,
            SentAt = now
        };

        try
        {
            await _daprClient.PublishEventAsync(
                DaprComponents.PubSubName,
                Topics.MessageSent,
                messageSentEvent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish message sent event for message {MessageId}", messageId);
            // Continue execution - message is already stored
        }

        _logger.LogInformation("Message {MessageId} sent by {SenderId}", messageId, request.SenderId);

        return new ChatMessageDto
        {
            Id = messageId,
            Content = cleanContent,
            SenderId = request.SenderId,
            SenderName = senderName,
            SentAt = now,
            Type = MessageType.Text
        };
    }

    public async Task<IEnumerable<ChatMessageDto>> GetRecentMessagesAsync(int count = 50)
    {
        if (count <= 0)
        {
            throw new ArgumentException("Count must be greater than zero");
        }

        var messageEntities = await _repository.GetRecentMessagesAsync(count);
        return messageEntities.Select(MapToDto);
    }

    public async Task<IEnumerable<ChatMessageDto>> GetMessageHistoryAsync(DateTime from, DateTime to)
    {
        if (from > to)
        {
            throw new ArgumentException("From date cannot be greater than to date");
        }

        var messageEntities = await _repository.GetMessagesByDateRangeAsync(from, to);
        return messageEntities.Select(MapToDto);
    }

    public async Task RemoveExpiredMessagesAsync()
    {
        var cutoffTime = DateTime.UtcNow.AddHours(-24);
        var expiredMessages = await _repository.GetExpiredMessagesAsync(cutoffTime);

        var messageIds = expiredMessages.Select(m => m.RowKey).ToList();
        if (messageIds.Count > 0)
        {
            await _repository.DeleteBatchAsync(messageIds);
            _logger.LogInformation("Removed {Count} expired messages", messageIds.Count);
        }
    }

    private static ChatMessageDto MapToDto(MessageEntityDto entity)
    {
        return new ChatMessageDto
        {
            Id = entity.RowKey,
            Content = entity.Content,
            SenderId = entity.SenderId,
            SenderName = entity.SenderName,
            SentAt = entity.SentAt,
            Type = entity.Type
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
