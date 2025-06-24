using Dapr.Client;
using HexMaster.Chat.Messages.Api.Entities;
using HexMaster.Chat.Messages.Api.Repositories;
using HexMaster.Chat.Messages.Api.Services;
using HexMaster.Chat.Shared.Constants;
using HexMaster.Chat.Shared.Events;
using HexMaster.Chat.Shared.Models;
using HexMaster.Chat.Shared.Requests;
using Microsoft.Extensions.Logging;
using Moq;

namespace HexMaster.Chat.Messages.Tests.Services;

public class MessageServiceTests
{
    private readonly Mock<IMessageRepository> _mockRepository;
    private readonly Mock<DaprClient> _mockDaprClient;
    private readonly Mock<IMemberStateService> _mockMemberStateService;
    private readonly Mock<ILogger<MessageService>> _mockLogger;
    private readonly MessageService _service;

    public MessageServiceTests()
    {
        _mockRepository = new Mock<IMessageRepository>();
        _mockDaprClient = new Mock<DaprClient>();
        _mockMemberStateService = new Mock<IMemberStateService>();
        _mockLogger = new Mock<ILogger<MessageService>>();

        _service = new MessageService(
            _mockRepository.Object,
            _mockDaprClient.Object,
            _mockMemberStateService.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task SendMessageAsync_WithValidMessage_ShouldCreateMessageAndPublishEvent()
    {
        // Arrange
        var request = new SendMessageRequest
        {
            Content = "Hello, world!",
            SenderId = "sender-123"
        };

        _mockMemberStateService
            .Setup(x => x.GetMemberNameAsync("sender-123"))
            .ReturnsAsync("Test User");

        _mockRepository
            .Setup(x => x.CreateAsync(It.IsAny<MessageEntity>()))
            .ReturnsAsync((MessageEntity entity) => entity);

        _mockDaprClient
            .Setup(x => x.PublishEventAsync(
                DaprComponents.PubSubName,
                Topics.MessageSent,
                It.IsAny<MessageSentEvent>(),
                default))
            .Returns(Task.CompletedTask);

        _mockMemberStateService
            .Setup(x => x.UpdateMemberActivityAsync("sender-123"))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.SendMessageAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Hello, world!", result.Content);
        Assert.Equal("sender-123", result.SenderId);
        Assert.Equal("Test User", result.SenderName);
        Assert.Equal(MessageType.Text, result.Type);
        Assert.False(string.IsNullOrEmpty(result.Id));

        _mockRepository.Verify(x => x.CreateAsync(It.Is<MessageEntity>(
            m => m.Content == "Hello, world!" &&
                 m.SenderId == "sender-123" &&
                 m.SenderName == "Test User")), Times.Once);

        _mockDaprClient.Verify(x => x.PublishEventAsync(
            DaprComponents.PubSubName,
            Topics.MessageSent,
            It.Is<MessageSentEvent>(e =>
                e.Content == "Hello, world!" &&
                e.SenderId == "sender-123" &&
                e.SenderName == "Test User"),
            default), Times.Once);

        _mockMemberStateService.Verify(x => x.UpdateMemberActivityAsync("sender-123"), Times.Once);
    }

    [Fact]
    public async Task SendMessageAsync_WithEmptyContent_ShouldThrowArgumentException()
    {
        // Arrange
        var request = new SendMessageRequest
        {
            Content = "",
            SenderId = "sender-123"
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => _service.SendMessageAsync(request));
        Assert.Contains("Message content cannot be empty", exception.Message);
    }

    [Fact]
    public async Task SendMessageAsync_WithWhitespaceContent_ShouldThrowArgumentException()
    {
        // Arrange
        var request = new SendMessageRequest
        {
            Content = "   ",
            SenderId = "sender-123"
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => _service.SendMessageAsync(request));
        Assert.Contains("Message content cannot be empty", exception.Message);
    }

    [Fact]
    public async Task SendMessageAsync_WithTooLongContent_ShouldThrowArgumentException()
    {
        // Arrange
        var request = new SendMessageRequest
        {
            Content = new string('a', 1001), // 1001 characters, exceeds limit
            SenderId = "sender-123"
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => _service.SendMessageAsync(request));
        Assert.Contains("Message content too long", exception.Message);
    }

    [Fact]
    public async Task SendMessageAsync_WithUnknownSender_ShouldUseFallbackName()
    {
        // Arrange
        var request = new SendMessageRequest
        {
            Content = "Hello from unknown sender",
            SenderId = "unknown-sender-12345"
        };

        _mockMemberStateService
            .Setup(x => x.GetMemberNameAsync("unknown-sender-12345"))
            .ReturnsAsync((string?)null);

        _mockRepository
            .Setup(x => x.CreateAsync(It.IsAny<MessageEntity>()))
            .ReturnsAsync((MessageEntity entity) => entity);

        _mockDaprClient
            .Setup(x => x.PublishEventAsync(
                DaprComponents.PubSubName,
                Topics.MessageSent,
                It.IsAny<MessageSentEvent>(),
                default))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.SendMessageAsync(request);

        // Assert
        Assert.Equal("User_unknown-", result.SenderName);

        // Verify warning was logged
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("not found in state store")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task SendMessageAsync_WithProfanity_ShouldFilterContent()
    {
        // Arrange
        var request = new SendMessageRequest
        {
            Content = "This is spam content",
            SenderId = "sender-123"
        };

        _mockMemberStateService
            .Setup(x => x.GetMemberNameAsync("sender-123"))
            .ReturnsAsync("Test User");

        _mockRepository
            .Setup(x => x.CreateAsync(It.IsAny<MessageEntity>()))
            .ReturnsAsync((MessageEntity entity) => entity);

        _mockDaprClient
            .Setup(x => x.PublishEventAsync(
                DaprComponents.PubSubName,
                Topics.MessageSent,
                It.IsAny<MessageSentEvent>(),
                default))
            .Returns(Task.CompletedTask);

        _mockMemberStateService
            .Setup(x => x.UpdateMemberActivityAsync("sender-123"))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.SendMessageAsync(request);

        // Assert
        Assert.Equal("This is *** content", result.Content);
    }

    [Fact]
    public async Task GetRecentMessagesAsync_ShouldReturnMappedMessages()
    {
        // Arrange
        var messageEntities = new List<MessageEntity>
        {
            new()
            {
                RowKey = "msg1",
                Content = "Message 1",
                SenderId = "sender1",
                SenderName = "User 1",
                SentAt = DateTime.UtcNow.AddMinutes(-5),
                MessageType = (int)MessageType.Text
            },
            new()
            {
                RowKey = "msg2",
                Content = "Message 2",
                SenderId = "sender2",
                SenderName = "User 2",
                SentAt = DateTime.UtcNow.AddMinutes(-3),
                MessageType = (int)MessageType.Text
            }
        };

        _mockRepository
            .Setup(x => x.GetRecentMessagesAsync(50))
            .ReturnsAsync(messageEntities);

        // Act
        var result = await _service.GetRecentMessagesAsync();

        // Assert
        Assert.Equal(2, result.Count());
        Assert.Contains(result, m => m.Id == "msg1" && m.Content == "Message 1");
        Assert.Contains(result, m => m.Id == "msg2" && m.Content == "Message 2");
    }

    [Fact]
    public async Task GetMessageHistoryAsync_ShouldReturnMessagesInDateRange()
    {
        // Arrange
        var from = DateTime.UtcNow.AddHours(-2);
        var to = DateTime.UtcNow;
        var messageEntities = new List<MessageEntity>
        {
            new()
            {
                RowKey = "msg1",
                Content = "Historical message",
                SenderId = "sender1",
                SenderName = "User 1",
                SentAt = from.AddMinutes(30),
                MessageType = (int)MessageType.Text
            }
        };

        _mockRepository
            .Setup(x => x.GetMessagesByDateRangeAsync(from, to))
            .ReturnsAsync(messageEntities);

        // Act
        var result = await _service.GetMessageHistoryAsync(from, to);

        // Assert
        Assert.Single(result);
        Assert.Equal("msg1", result.First().Id);
        Assert.Equal("Historical message", result.First().Content);
    }

    [Fact]
    public async Task RemoveExpiredMessagesAsync_ShouldRemoveOldMessages()
    {
        // Arrange
        var cutoffTime = DateTime.UtcNow.AddHours(-24);
        var expiredMessages = new List<MessageEntity>
        {
            new() { RowKey = "expired1" },
            new() { RowKey = "expired2" },
            new() { RowKey = "expired3" }
        };

        _mockRepository
            .Setup(x => x.GetExpiredMessagesAsync(It.Is<DateTime>(d => d <= cutoffTime.AddMinutes(1))))
            .ReturnsAsync(expiredMessages);

        _mockRepository
            .Setup(x => x.DeleteBatchAsync(It.IsAny<IEnumerable<string>>()))
            .Returns(Task.CompletedTask);

        // Act
        await _service.RemoveExpiredMessagesAsync();

        // Assert
        _mockRepository.Verify(x => x.DeleteBatchAsync(
            It.Is<IEnumerable<string>>(ids =>
                ids.Contains("expired1") &&
                ids.Contains("expired2") &&
                ids.Contains("expired3"))), Times.Once);
    }

    [Fact]
    public async Task RemoveExpiredMessagesAsync_WhenNoExpiredMessages_ShouldNotDeleteAnything()
    {
        // Arrange
        _mockRepository
            .Setup(x => x.GetExpiredMessagesAsync(It.IsAny<DateTime>()))
            .ReturnsAsync(new List<MessageEntity>());

        // Act
        await _service.RemoveExpiredMessagesAsync();

        // Assert
        _mockRepository.Verify(x => x.DeleteBatchAsync(It.IsAny<IEnumerable<string>>()), Times.Never);
    }
}
