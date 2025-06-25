using Azure;
using HexMaster.Chat.Members.Entities;

namespace HexMaster.Chat.Members.Tests.Entities;

public class MemberEntityTests
{
    [Fact]
    public void MemberEntity_DefaultValues_ShouldBeInitializedCorrectly()
    {
        // Arrange & Act
        var entity = new MemberEntity();

        // Assert
        Assert.Equal("member", entity.PartitionKey);
        Assert.Equal(string.Empty, entity.RowKey);
        Assert.Equal(string.Empty, entity.Name);
        Assert.Equal(string.Empty, entity.Email);
        Assert.False(entity.IsActive);
        Assert.Equal(default(DateTime), entity.JoinedAt);
        Assert.Equal(default(DateTime), entity.LastActivityAt);
        Assert.Equal(default(ETag), entity.ETag);
        Assert.Null(entity.Timestamp);
    }

    [Fact]
    public void MemberEntity_SetProperties_ShouldUpdateCorrectly()
    {
        // Arrange
        var entity = new MemberEntity();
        var testDate = DateTime.UtcNow;
        var testETag = new ETag("test-etag");

        // Act
        entity.RowKey = "test-id";
        entity.Name = "Test User";
        entity.Email = "test@example.com";
        entity.JoinedAt = testDate;
        entity.LastActivityAt = testDate;
        entity.IsActive = true;
        entity.ETag = testETag;

        // Assert
        Assert.Equal("test-id", entity.RowKey);
        Assert.Equal("Test User", entity.Name);
        Assert.Equal("test@example.com", entity.Email);
        Assert.Equal(testDate, entity.JoinedAt);
        Assert.Equal(testDate, entity.LastActivityAt);
        Assert.True(entity.IsActive);
        Assert.Equal(testETag, entity.ETag);
    }
}
