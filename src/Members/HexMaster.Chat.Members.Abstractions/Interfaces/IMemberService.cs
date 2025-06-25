using HexMaster.Chat.Members.Abstractions.DTOs;
using HexMaster.Chat.Members.Abstractions.Requests;

namespace HexMaster.Chat.Members.Abstractions.Interfaces;

public interface IMemberService
{
    Task<ChatMemberDto> RegisterMemberAsync(RegisterMemberRequest request);
    Task<ChatMemberDto?> GetMemberAsync(string id);
    Task UpdateLastActivityAsync(string id);
    Task RemoveInactiveMembersAsync();
}
