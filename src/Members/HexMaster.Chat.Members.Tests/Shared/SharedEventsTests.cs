using HexMaster.Chat.Shared.Events;

namespace HexMaster.Chat.Members.Tests.Shared;

public class SharedEventsTests
{
    [Fact]
    public void MemberJoinedEvent_DefaultValues_ShouldBeInitializedCorrectly()
    {
        // Arrange & Act
        var eventObj = new MemberJoinedEvent();

        // Assert
        Assert.Equal(string.Empty, eventObj.Id);
        Assert.Equal(string.Empty, eventObj.Name);
        Assert.Equal(string.Empty, eventObj.Email);
        Assert.Equal(default(DateTime), eventObj.JoinedAt);
    }

    [Fact]
    public void MemberJoinedEvent_WithValues_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var now = DateTime.UtcNow;

        // Act
        var eventObj = new MemberJoinedEvent
        {
            Id = "member-123",
            Name = "Test User",
            Email = "test@example.com",
            JoinedAt = now
        };

        // Assert
        Assert.Equal("member-123", eventObj.Id);
        Assert.Equal("Test User", eventObj.Name);
        Assert.Equal("test@example.com", eventObj.Email);
        Assert.Equal(now, eventObj.JoinedAt);
    }

    [Fact]
    public void MemberLeftEvent_DefaultValues_ShouldBeInitializedCorrectly()
    {
        // Arrange & Act
        var eventObj = new MemberLeftEvent();

        // Assert
        Assert.Equal(string.Empty, eventObj.Id);
        Assert.Equal(string.Empty, eventObj.Name);
        Assert.Equal(default(DateTime), eventObj.LeftAt);
    }

    [Fact]
    public void MemberLeftEvent_WithValues_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var now = DateTime.UtcNow;

        // Act
        var eventObj = new MemberLeftEvent
        {
            Id = "member-123",
            Name = "Test User",
            LeftAt = now
        };

        // Assert
        Assert.Equal("member-123", eventObj.Id);
        Assert.Equal("Test User", eventObj.Name);
        Assert.Equal(now, eventObj.LeftAt);
    }

    [Fact]
    public void MessageSentEvent_DefaultValues_ShouldBeInitializedCorrectly()
    {
        // Arrange & Act
        var eventObj = new MessageSentEvent();

        // Assert
        Assert.Equal(string.Empty, eventObj.Id);
        Assert.Equal(string.Empty, eventObj.Content);
        Assert.Equal(string.Empty, eventObj.SenderId);
        Assert.Equal(string.Empty, eventObj.SenderName);
        Assert.Equal(default(DateTime), eventObj.SentAt);
    }

    [Fact]
    public void MessageSentEvent_WithValues_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var now = DateTime.UtcNow;

        // Act
        var eventObj = new MessageSentEvent
        {
            Id = "msg-123",
            Content = "Hello, world!",
            SenderId = "sender-456",
            SenderName = "Test Sender",
            SentAt = now
        };

        // Assert
        Assert.Equal("msg-123", eventObj.Id);
        Assert.Equal("Hello, world!", eventObj.Content);
        Assert.Equal("sender-456", eventObj.SenderId);
        Assert.Equal("Test Sender", eventObj.SenderName);
        Assert.Equal(now, eventObj.SentAt);
    }
}
