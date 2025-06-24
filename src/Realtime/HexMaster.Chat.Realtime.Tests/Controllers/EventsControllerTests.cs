using HexMaster.Chat.Realtime.Api.Controllers;
using HexMaster.Chat.Realtime.Api.Hubs;
using HexMaster.Chat.Shared.Events;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Moq;

namespace HexMaster.Chat.Realtime.Tests.Controllers;

public class EventsControllerTests
{
    private readonly Mock<IHubContext<ChatHub, IChatClient>> _mockHubContext;
    private readonly Mock<ILogger<EventsController>> _mockLogger;
    private readonly Mock<IChatClient> _mockChatClient;
    private readonly EventsController _controller;

    public EventsControllerTests()
    {
        _mockHubContext = new Mock<IHubContext<ChatHub, IChatClient>>();
        _mockLogger = new Mock<ILogger<EventsController>>();
        _mockChatClient = new Mock<IChatClient>();

        var mockClients = new Mock<IHubCallerClients<IChatClient>>();
        mockClients.Setup(x => x.Group("ChatRoom")).Returns(_mockChatClient.Object);

        _mockHubContext.Setup(x => x.Clients).Returns(mockClients.Object);

        _controller = new EventsController(_mockHubContext.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task OnMessageSent_WithValidEvent_ShouldBroadcastMessageAndReturnOk()
    {
        // Arrange
        var messageSentEvent = new MessageSentEvent
        {
            Id = "msg-123",
            Content = "Hello, world!",
            SenderId = "sender-456",
            SenderName = "Test Sender",
            SentAt = DateTime.UtcNow
        };

        _mockChatClient
            .Setup(x => x.ReceiveMessage(
                messageSentEvent.Id,
                messageSentEvent.Content,
                messageSentEvent.SenderId,
                messageSentEvent.SenderName,
                messageSentEvent.SentAt))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.OnMessageSent(messageSentEvent);

        // Assert
        Assert.IsType<OkResult>(result);

        _mockChatClient.Verify(x => x.ReceiveMessage(
            messageSentEvent.Id,
            messageSentEvent.Content,
            messageSentEvent.SenderId,
            messageSentEvent.SenderName,
            messageSentEvent.SentAt), Times.Once);

        // Verify information logging
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Broadcasting message")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task OnMemberJoined_WithValidEvent_ShouldBroadcastMemberJoinedAndReturnOk()
    {
        // Arrange
        var memberJoinedEvent = new MemberJoinedEvent
        {
            Id = "member-123",
            Name = "Test User",
            Email = "test@example.com",
            JoinedAt = DateTime.UtcNow
        };

        _mockChatClient
            .Setup(x => x.MemberJoined(
                memberJoinedEvent.Id,
                memberJoinedEvent.Name,
                memberJoinedEvent.Email,
                memberJoinedEvent.JoinedAt))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.OnMemberJoined(memberJoinedEvent);

        // Assert
        Assert.IsType<OkResult>(result);

        _mockChatClient.Verify(x => x.MemberJoined(
            memberJoinedEvent.Id,
            memberJoinedEvent.Name,
            memberJoinedEvent.Email,
            memberJoinedEvent.JoinedAt), Times.Once);

        // Verify information logging
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Broadcasting member joined")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task OnMemberLeft_WithValidEvent_ShouldBroadcastMemberLeftAndReturnOk()
    {
        // Arrange
        var memberLeftEvent = new MemberLeftEvent
        {
            Id = "member-123",
            Name = "Test User",
            LeftAt = DateTime.UtcNow
        };

        _mockChatClient
            .Setup(x => x.MemberLeft(
                memberLeftEvent.Id,
                memberLeftEvent.Name,
                memberLeftEvent.LeftAt))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.OnMemberLeft(memberLeftEvent);

        // Assert
        Assert.IsType<OkResult>(result);

        _mockChatClient.Verify(x => x.MemberLeft(
            memberLeftEvent.Id,
            memberLeftEvent.Name,
            memberLeftEvent.LeftAt), Times.Once);

        // Verify information logging
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Broadcasting member left")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
