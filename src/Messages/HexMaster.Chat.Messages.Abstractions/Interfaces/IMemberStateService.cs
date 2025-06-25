using HexMaster.Chat.Members.Abstractions.Events;

namespace HexMaster.Chat.Messages.Abstractions.Interfaces;

public interface IMemberStateService
{
    Task StoreMemberAsync(MemberJoinedEvent memberEvent);
    Task<string?> GetMemberNameAsync(string memberId);
    Task UpdateMemberActivityAsync(string memberId);
}
