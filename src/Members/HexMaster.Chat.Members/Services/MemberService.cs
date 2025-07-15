using Dapr.Client;
using HexMaster.Chat.Members.Abstractions.DTOs;
using HexMaster.Chat.Members.Abstractions.Events;
using HexMaster.Chat.Members.Abstractions.Interfaces;
using HexMaster.Chat.Members.Abstractions.Requests;
using HexMaster.Chat.Shared.Constants;
using Microsoft.Extensions.Logging;

namespace HexMaster.Chat.Members.Services;

public class MemberService : IMemberService
{
    private readonly IMemberRepository _repository;
    private readonly DaprClient _daprClient;
    private readonly ILogger<MemberService> _logger;

    public MemberService(
        IMemberRepository repository,
        DaprClient daprClient,
        ILogger<MemberService> logger)
    {
        _repository = repository;
        _daprClient = daprClient;
        _logger = logger;
    }

    public async Task<ChatMemberDto> RegisterMemberAsync(RegisterMemberRequest request)
    {
        var memberId = Guid.NewGuid().ToString();
        var now = DateTime.UtcNow;

        var memberEntity = new MemberEntityDto
        {
            RowKey = memberId,
            Name = request.Name,
            JoinedAt = now,
            LastActivityAt = now,
            IsActive = true
        };

        await _repository.CreateAsync(memberEntity);

        // Publish member joined event
        var memberJoinedEvent = new MemberJoinedEvent
        {
            Id = memberId,
            Name = request.Name,
            JoinedAt = now
        };

        await _daprClient.PublishEventAsync(
            DaprComponents.PubSubName,
            Topics.MemberJoined,
            memberJoinedEvent);

        _logger.LogInformation("Member {MemberId} registered: {MemberName}", memberId, request.Name);

        return new ChatMemberDto
        {
            Id = memberId,
            Name = request.Name,
            JoinedAt = now,
            LastActivityAt = now,
            IsActive = true
        };
    }

    public async Task<ChatMemberDto?> GetMemberAsync(string id)
    {
        var memberEntity = await _repository.GetByIdAsync(id);
        if (memberEntity == null)
            return null;

        return new ChatMemberDto
        {
            Id = memberEntity.RowKey,
            Name = memberEntity.Name,
            JoinedAt = memberEntity.JoinedAt,
            LastActivityAt = memberEntity.LastActivityAt,
            IsActive = memberEntity.IsActive
        };
    }

    public async Task UpdateLastActivityAsync(string id)
    {
        var memberEntity = await _repository.GetByIdAsync(id);
        if (memberEntity != null)
        {
            var updatedEntity = memberEntity with { LastActivityAt = DateTime.UtcNow };
            await _repository.UpdateAsync(updatedEntity);
        }
    }

    public async Task RemoveInactiveMembersAsync()
    {
        var cutoffTime = DateTime.UtcNow.AddHours(-1);
        var inactiveMembers = await _repository.GetInactiveMembersAsync(cutoffTime);

        foreach (var member in inactiveMembers)
        {
            // Publish member left event
            var memberLeftEvent = new MemberLeftEvent
            {
                Id = member.RowKey,
                Name = member.Name,
                LeftAt = DateTime.UtcNow
            };

            await _daprClient.PublishEventAsync(
                DaprComponents.PubSubName,
                Topics.MemberLeft,
                memberLeftEvent);

            await _repository.DeleteAsync(member.RowKey);

            _logger.LogInformation("Removed inactive member {MemberId}: {MemberName}", member.RowKey, member.Name);
        }
    }
}
