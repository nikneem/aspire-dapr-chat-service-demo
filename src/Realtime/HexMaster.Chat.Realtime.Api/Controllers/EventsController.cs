using Dapr;
using HexMaster.Chat.Realtime.Api.Hubs;
using HexMaster.Chat.Shared.Constants;
using HexMaster.Chat.Shared.Events;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace HexMaster.Chat.Realtime.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EventsController(
    IHubContext<ChatHub, IChatClient> hubContext,
    ILogger<EventsController> logger)
    : ControllerBase
{
    [Topic(DaprComponents.PubSubName, Topics.MessageSent)]
    [HttpPost("message-sent")]
    public async Task<IActionResult> OnMessageSent([FromBody] MessageSentEvent messageSentEvent)
    {
        logger.LogInformation("Broadcasting message {MessageId} from {SenderId}",
            messageSentEvent.Id, messageSentEvent.SenderId);

        await hubContext.Clients.Group("ChatRoom").ReceiveMessage(
            messageSentEvent.Id,
            messageSentEvent.Content,
            messageSentEvent.SenderId,
            messageSentEvent.SenderName,
            messageSentEvent.SentAt);

        return Ok();
    }

    [Topic(DaprComponents.PubSubName, Topics.MemberJoined)]
    [HttpPost("member-joined")]
    public async Task<IActionResult> OnMemberJoined([FromBody] MemberJoinedEvent memberJoinedEvent)
    {
        logger.LogInformation("Broadcasting member joined: {MemberId} - {MemberName}",
            memberJoinedEvent.Id, memberJoinedEvent.Name);

        await hubContext.Clients.Group("ChatRoom").MemberJoined(
            memberJoinedEvent.Id,
            memberJoinedEvent.Name,
            memberJoinedEvent.Email,
            memberJoinedEvent.JoinedAt);

        return Ok();
    }

    [Topic(DaprComponents.PubSubName, Topics.MemberLeft)]
    [HttpPost("member-left")]
    public async Task<IActionResult> OnMemberLeft([FromBody] MemberLeftEvent memberLeftEvent)
    {
        logger.LogInformation("Broadcasting member left: {MemberId} - {MemberName}",
            memberLeftEvent.Id, memberLeftEvent.Name);

        await hubContext.Clients.Group("ChatRoom").MemberLeft(
            memberLeftEvent.Id,
            memberLeftEvent.Name,
            memberLeftEvent.LeftAt);

        return Ok();
    }
}
