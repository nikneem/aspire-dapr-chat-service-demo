using HexMaster.Chat.Shared.Requests;

namespace HexMaster.Chat.Members.Tests.Shared;

public class SharedRequestsTests
{
    [Fact]
    public void RegisterMemberRequest_DefaultValues_ShouldBeInitializedCorrectly()
    {
        // Arrange & Act
        var request = new RegisterMemberRequest();

        // Assert
        Assert.Equal(string.Empty, request.Name);
        Assert.Equal(string.Empty, request.Email);
    }

    [Fact]
    public void RegisterMemberRequest_WithValues_ShouldSetPropertiesCorrectly()
    {
        // Act
        var request = new RegisterMemberRequest
        {
            Name = "Test User",
            Email = "test@example.com"
        };

        // Assert
        Assert.Equal("Test User", request.Name);
        Assert.Equal("test@example.com", request.Email);
    }

    [Fact]
    public void SendMessageRequest_DefaultValues_ShouldBeInitializedCorrectly()
    {
        // Arrange & Act
        var request = new SendMessageRequest();

        // Assert
        Assert.Equal(string.Empty, request.Content);
        Assert.Equal(string.Empty, request.SenderId);
    }

    [Fact]
    public void SendMessageRequest_WithValues_ShouldSetPropertiesCorrectly()
    {
        // Act
        var request = new SendMessageRequest
        {
            Content = "Hello, world!",
            SenderId = "sender-123"
        };

        // Assert
        Assert.Equal("Hello, world!", request.Content);
        Assert.Equal("sender-123", request.SenderId);
    }
}
