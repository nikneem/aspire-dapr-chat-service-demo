using HexMaster.Chat.Messages.Api.Controllers;
using HexMaster.Chat.Messages.Api.Services;
using HexMaster.Chat.Shared.Events;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace HexMaster.Chat.Messages.Tests.Controllers;

public class EventsControllerTests
{
    private readonly Mock<IMemberStateService> _mockMemberStateService;
    private readonly Mock<ILogger<EventsController>> _mockLogger;
    private readonly EventsController _controller;

    public EventsControllerTests()
    {
        _mockMemberStateService = new Mock<IMemberStateService>();
        _mockLogger = new Mock<ILogger<EventsController>>();
        _controller = new EventsController(_mockMemberStateService.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task HandleMemberJoined_WithValidEvent_ShouldReturnOk()
    {
        // Arrange
        var memberEvent = new MemberJoinedEvent
        {
            Id = "member-123",
            Name = "Test User",
            Email = "test@example.com",
            JoinedAt = DateTime.UtcNow
        };

        _mockMemberStateService
            .Setup(x => x.StoreMemberAsync(memberEvent))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.HandleMemberJoined(memberEvent);

        // Assert
        Assert.IsType<OkResult>(result);

        _mockMemberStateService.Verify(x => x.StoreMemberAsync(memberEvent), Times.Once);

        // Verify information logging
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Processed member joined event")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleMemberJoined_WhenServiceThrows_ShouldReturnInternalServerError()
    {
        // Arrange
        var memberEvent = new MemberJoinedEvent
        {
            Id = "member-123",
            Name = "Test User",
            Email = "test@example.com",
            JoinedAt = DateTime.UtcNow
        };

        var exception = new Exception("Storage service error");
        _mockMemberStateService
            .Setup(x => x.StoreMemberAsync(memberEvent))
            .ThrowsAsync(exception);

        // Act
        var result = await _controller.HandleMemberJoined(memberEvent);

        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, statusCodeResult.StatusCode);
        Assert.Equal("Failed to process member joined event", statusCodeResult.Value);

        // Verify error logging
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error processing member joined event")),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleMemberJoined_WithNullEvent_ShouldStillProcessCorrectly()
    {
        // Arrange
        var memberEvent = new MemberJoinedEvent();

        _mockMemberStateService
            .Setup(x => x.StoreMemberAsync(memberEvent))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.HandleMemberJoined(memberEvent);

        // Assert
        Assert.IsType<OkResult>(result);
        _mockMemberStateService.Verify(x => x.StoreMemberAsync(memberEvent), Times.Once);
    }
}
