using HexMaster.Chat.Shared.Models;

namespace HexMaster.Chat.Members.Tests.Shared;

public class SharedModelsTests
{
    [Fact]
    public void ChatMember_DefaultValues_ShouldBeInitializedCorrectly()
    {
        // Arrange & Act
        var member = new ChatMember();

        // Assert
        Assert.Equal(string.Empty, member.Id);
        Assert.Equal(string.Empty, member.Name);
        Assert.Equal(default(DateTime), member.JoinedAt);
        Assert.Equal(default(DateTime), member.LastActivityAt);
        Assert.False(member.IsActive);
    }

    [Fact]
    public void ChatMember_WithValues_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var now = DateTime.UtcNow;

        // Act
        var member = new ChatMember
        {
            Id = "member-123",
            Name = "Test User",
            JoinedAt = now,
            LastActivityAt = now,
            IsActive = true
        };

        // Assert
        Assert.Equal("member-123", member.Id);
        Assert.Equal("Test User", member.Name);
        Assert.Equal(now, member.JoinedAt);
        Assert.Equal(now, member.LastActivityAt);
        Assert.True(member.IsActive);
    }

    [Fact]
    public void ChatMessage_DefaultValues_ShouldBeInitializedCorrectly()
    {
        // Arrange & Act
        var message = new ChatMessage();

        // Assert
        Assert.Equal(string.Empty, message.Id);
        Assert.Equal(string.Empty, message.Content);
        Assert.Equal(string.Empty, message.SenderId);
        Assert.Equal(string.Empty, message.SenderName);
        Assert.Equal(default(DateTime), message.SentAt);
        Assert.Equal(MessageType.Text, message.Type);
    }

    [Fact]
    public void ChatMessage_WithValues_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var now = DateTime.UtcNow;

        // Act
        var message = new ChatMessage
        {
            Id = "msg-123",
            Content = "Hello, world!",
            SenderId = "sender-456",
            SenderName = "Test Sender",
            SentAt = now,
            Type = MessageType.System
        };

        // Assert
        Assert.Equal("msg-123", message.Id);
        Assert.Equal("Hello, world!", message.Content);
        Assert.Equal("sender-456", message.SenderId);
        Assert.Equal("Test Sender", message.SenderName);
        Assert.Equal(now, message.SentAt);
        Assert.Equal(MessageType.System, message.Type);
    }

    [Fact]
    public void MessageType_ShouldHaveCorrectValues()
    {
        // Assert
        Assert.Equal(0, (int)MessageType.Text);
        Assert.Equal(1, (int)MessageType.System);
        Assert.Equal(2, (int)MessageType.Emoji);
    }
}
