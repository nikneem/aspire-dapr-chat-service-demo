using Azure;
using HexMaster.Chat.Messages.Api.Entities;

namespace HexMaster.Chat.Messages.Tests.Entities;

public class MessageEntityTests
{
    [Fact]
    public void MessageEntity_DefaultValues_ShouldBeInitializedCorrectly()
    {
        // Arrange & Act
        var entity = new MessageEntity();

        // Assert
        Assert.Equal("message", entity.PartitionKey);
        Assert.Equal(string.Empty, entity.RowKey);
        Assert.Equal(string.Empty, entity.Content);
        Assert.Equal(string.Empty, entity.SenderId);
        Assert.Equal(string.Empty, entity.SenderName);
        Assert.Equal(default(DateTime), entity.SentAt);
        Assert.Equal(0, entity.MessageType);
        Assert.Equal(default(ETag), entity.ETag);
        Assert.Null(entity.Timestamp);
    }

    [Fact]
    public void MessageEntity_SetProperties_ShouldUpdateCorrectly()
    {
        // Arrange
        var entity = new MessageEntity();
        var testDate = DateTime.UtcNow;
        var testETag = new ETag("test-etag");

        // Act
        entity.RowKey = "test-id";
        entity.Content = "Test message content";
        entity.SenderId = "sender-123";
        entity.SenderName = "Test Sender";
        entity.SentAt = testDate;
        entity.MessageType = 1;
        entity.ETag = testETag;

        // Assert
        Assert.Equal("test-id", entity.RowKey);
        Assert.Equal("Test message content", entity.Content);
        Assert.Equal("sender-123", entity.SenderId);
        Assert.Equal("Test Sender", entity.SenderName);
        Assert.Equal(testDate, entity.SentAt);
        Assert.Equal(1, entity.MessageType);
        Assert.Equal(testETag, entity.ETag);
    }
}
