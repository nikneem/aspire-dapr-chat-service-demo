using Microsoft.AspNetCore.SignalR;

namespace HexMaster.Chat.Realtime.Api.Hubs;

public interface IChatClient
{
    Task ReceiveMessage(string id, string content, string senderId, string senderName, DateTime sentAt);
    Task MemberJoined(string id, string name, string email, DateTime joinedAt);
    Task MemberLeft(string id, string name, DateTime leftAt);
}

public class ChatHub : Hub<IChatClient>
{
    private readonly ILogger<ChatHub> _logger;

    public ChatHub(ILogger<ChatHub> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation("Client connected: {ConnectionId}", Context.ConnectionId);
        await Groups.AddToGroupAsync(Context.ConnectionId, "ChatRoom");
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation("Client disconnected: {ConnectionId}", Context.ConnectionId);
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, "ChatRoom");
        await base.OnDisconnectedAsync(exception);
    }

    public async Task JoinChatRoom(string userId, string userName)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "ChatRoom");
        _logger.LogInformation("User {UserId} ({UserName}) joined chat room", userId, userName);
    }

    public async Task LeaveChatRoom(string userId, string userName)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, "ChatRoom");
        _logger.LogInformation("User {UserId} ({UserName}) left chat room", userId, userName);
    }
}
