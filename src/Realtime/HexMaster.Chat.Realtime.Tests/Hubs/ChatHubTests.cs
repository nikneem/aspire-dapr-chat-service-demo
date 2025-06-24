using HexMaster.Chat.Realtime.Api.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Moq;

namespace HexMaster.Chat.Realtime.Tests.Hubs;

public class ChatHubTests
{
    private readonly Mock<ILogger<ChatHub>> _mockLogger;
    private readonly Mock<IHubContext> _mockHubContext;
    private readonly Mock<HubCallerContext> _mockContext;
    private readonly Mock<IGroupManager> _mockGroups;
    private readonly ChatHub _hub;

    public ChatHubTests()
    {
        _mockLogger = new Mock<ILogger<ChatHub>>();
        _mockHubContext = new Mock<IHubContext>();
        _mockContext = new Mock<HubCallerContext>();
        _mockGroups = new Mock<IGroupManager>();

        _hub = new ChatHub(_mockLogger.Object);

        // Setup the hub context
        _hub.Context = _mockContext.Object;
        _hub.Groups = _mockGroups.Object;
    }

    [Fact]
    public async Task OnConnectedAsync_ShouldAddToGroupAndLogConnection()
    {
        // Arrange
        var connectionId = "test-connection-id";
        _mockContext.Setup(x => x.ConnectionId).Returns(connectionId);
        _mockGroups
            .Setup(x => x.AddToGroupAsync(connectionId, "ChatRoom", default))
            .Returns(Task.CompletedTask);

        // Act
        await _hub.OnConnectedAsync();

        // Assert
        _mockGroups.Verify(x => x.AddToGroupAsync(connectionId, "ChatRoom", default), Times.Once);

        // Verify logging
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Client connected")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task OnDisconnectedAsync_ShouldRemoveFromGroupAndLogDisconnection()
    {
        // Arrange
        var connectionId = "test-connection-id";
        var exception = new Exception("Test exception");

        _mockContext.Setup(x => x.ConnectionId).Returns(connectionId);
        _mockGroups
            .Setup(x => x.RemoveFromGroupAsync(connectionId, "ChatRoom", default))
            .Returns(Task.CompletedTask);

        // Act
        await _hub.OnDisconnectedAsync(exception);

        // Assert
        _mockGroups.Verify(x => x.RemoveFromGroupAsync(connectionId, "ChatRoom", default), Times.Once);

        // Verify logging
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Client disconnected")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task OnDisconnectedAsync_WithNullException_ShouldStillWork()
    {
        // Arrange
        var connectionId = "test-connection-id";

        _mockContext.Setup(x => x.ConnectionId).Returns(connectionId);
        _mockGroups
            .Setup(x => x.RemoveFromGroupAsync(connectionId, "ChatRoom", default))
            .Returns(Task.CompletedTask);

        // Act
        await _hub.OnDisconnectedAsync(null);

        // Assert
        _mockGroups.Verify(x => x.RemoveFromGroupAsync(connectionId, "ChatRoom", default), Times.Once);
    }

    [Fact]
    public async Task JoinChatRoom_ShouldAddToGroupAndLogJoin()
    {
        // Arrange
        var connectionId = "test-connection-id";
        var userId = "user-123";
        var userName = "Test User";

        _mockContext.Setup(x => x.ConnectionId).Returns(connectionId);
        _mockGroups
            .Setup(x => x.AddToGroupAsync(connectionId, "ChatRoom", default))
            .Returns(Task.CompletedTask);

        // Act
        await _hub.JoinChatRoom(userId, userName);

        // Assert
        _mockGroups.Verify(x => x.AddToGroupAsync(connectionId, "ChatRoom", default), Times.Once);

        // Verify logging
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("joined chat room")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task LeaveChatRoom_ShouldRemoveFromGroupAndLogLeave()
    {
        // Arrange
        var connectionId = "test-connection-id";
        var userId = "user-123";
        var userName = "Test User";

        _mockContext.Setup(x => x.ConnectionId).Returns(connectionId);
        _mockGroups
            .Setup(x => x.RemoveFromGroupAsync(connectionId, "ChatRoom", default))
            .Returns(Task.CompletedTask);

        // Act
        await _hub.LeaveChatRoom(userId, userName);

        // Assert
        _mockGroups.Verify(x => x.RemoveFromGroupAsync(connectionId, "ChatRoom", default), Times.Once);

        // Verify logging
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("left chat room")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
