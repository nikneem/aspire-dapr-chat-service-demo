using Dapr;
using HexMaster.Chat.Messages.Api.Services;
using HexMaster.Chat.Shared.Constants;
using HexMaster.Chat.Shared.Events;
using Microsoft.AspNetCore.Mvc;

namespace HexMaster.Chat.Messages.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EventsController : ControllerBase
{
    private readonly IMemberStateService _memberStateService;
    private readonly ILogger<EventsController> _logger;

    public EventsController(
        IMemberStateService memberStateService,
        ILogger<EventsController> logger)
    {
        _memberStateService = memberStateService;
        _logger = logger;
    }

    [HttpPost("member-joined")]
    [Topic(DaprComponents.PubSubName, Topics.MemberJoined)]
    public async Task<IActionResult> HandleMemberJoined([FromBody] MemberJoinedEvent memberEvent)
    {
        try
        {
            await _memberStateService.StoreMemberAsync(memberEvent);

            _logger.LogInformation("Processed member joined event for {MemberId} ({MemberName})",
                memberEvent.Id, memberEvent.Name);

            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing member joined event for {MemberId}", memberEvent.Id);
            return StatusCode(500, "Failed to process member joined event");
        }
    }
}
