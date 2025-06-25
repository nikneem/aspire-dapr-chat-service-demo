using Dapr.Client;
using HexMaster.Chat.Messages.Abstractions.DTOs;
using HexMaster.Chat.Messages.Abstractions.Interfaces;
using HexMaster.Chat.Members.Abstractions.Events;
using HexMaster.Chat.Shared.Constants;
using Microsoft.Extensions.Logging;

namespace HexMaster.Chat.Messages.Services;

public class MemberStateService : IMemberStateService
{
    private readonly DaprClient _daprClient;
    private readonly ILogger<MemberStateService> _logger;
    private static readonly TimeSpan MemberCacheExpiration = TimeSpan.FromMinutes(60);

    public MemberStateService(DaprClient daprClient, ILogger<MemberStateService> logger)
    {
        _daprClient = daprClient;
        _logger = logger;
    }

    public async Task StoreMemberAsync(MemberJoinedEvent memberEvent)
    {
        var memberData = new MemberCacheDataDto
        {
            Id = memberEvent.Id,
            Name = memberEvent.Name,
            Email = memberEvent.Email,
            JoinedAt = memberEvent.JoinedAt,
            LastAccessAt = DateTime.UtcNow
        };

        var metadata = new Dictionary<string, string>
        {
            ["ttlInSeconds"] = ((int)MemberCacheExpiration.TotalSeconds).ToString()
        };

        await _daprClient.SaveStateAsync(
            DaprComponents.StateStoreName,
            GetMemberStateKey(memberEvent.Id),
            memberData,
            metadata: metadata);

        _logger.LogInformation("Stored member {MemberId} ({MemberName}) in state store with 60-minute expiration",
            memberEvent.Id, memberEvent.Name);
    }

    public async Task<string?> GetMemberNameAsync(string memberId)
    {
        try
        {
            var memberData = await _daprClient.GetStateAsync<MemberCacheData>(
                DaprComponents.StateStoreName,
                GetMemberStateKey(memberId));

            if (memberData != null)
            {
                // Update the sliding expiration by touching the state
                await UpdateMemberActivityAsync(memberId);
                return memberData.Name;
            }

            _logger.LogWarning("Member {MemberId} not found in state store", memberId);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving member {MemberId} from state store", memberId);
            return null;
        }
    }

    public async Task UpdateMemberActivityAsync(string memberId)
    {
        try
        {
            var memberData = await _daprClient.GetStateAsync<MemberCacheData>(
                DaprComponents.StateStoreName,
                GetMemberStateKey(memberId));

            if (memberData != null)
            {
                // Update last access time for sliding expiration
                memberData.LastAccessAt = DateTime.UtcNow;

                var metadata = new Dictionary<string, string>
                {
                    ["ttlInSeconds"] = ((int)MemberCacheExpiration.TotalSeconds).ToString()
                };

                await _daprClient.SaveStateAsync(
                    DaprComponents.StateStoreName,
                    GetMemberStateKey(memberId),
                    memberData,
                    metadata: metadata);

                _logger.LogDebug("Updated activity for member {MemberId}, extending expiration", memberId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating member activity for {MemberId}", memberId);
        }
    }

    private static string GetMemberStateKey(string memberId) => $"member:{memberId}";

    private sealed class MemberCacheData
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime JoinedAt { get; set; }
        public DateTime LastAccessAt { get; set; }
    }
}
