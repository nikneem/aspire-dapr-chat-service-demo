namespace HexMaster.Chat.Shared.Constants;

public static class DaprComponents
{
    public const string PubSubName = "chatservice-pubsub";
    public const string StateStoreName = "chatservice-statestore";
}

public static class Topics
{
    public const string MemberJoined = "member-joined";
    public const string MemberLeft = "member-left";
    public const string MessageSent = "message-sent";
}

public static class TableNames
{
    public const string Members = "members";
    public const string Messages = "messages";
}
