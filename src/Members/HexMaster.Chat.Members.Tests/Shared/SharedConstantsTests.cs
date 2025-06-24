using HexMaster.Chat.Shared.Constants;

namespace HexMaster.Chat.Members.Tests.Shared;

public class SharedConstantsTests
{
    [Fact]
    public void DaprComponents_ShouldHaveCorrectValues()
    {
        // Assert
        Assert.Equal("chatservice-pubsub", DaprComponents.PubSubName);
        Assert.Equal("chatservice-statestore", DaprComponents.StateStoreName);
    }

    [Fact]
    public void Topics_ShouldHaveCorrectValues()
    {
        // Assert
        Assert.Equal("member-joined", Topics.MemberJoined);
        Assert.Equal("member-left", Topics.MemberLeft);
        Assert.Equal("message-sent", Topics.MessageSent);
    }

    [Fact]
    public void TableNames_ShouldHaveCorrectValues()
    {
        // Assert
        Assert.Equal("members", TableNames.Members);
        Assert.Equal("messages", TableNames.Messages);
    }
}
