using HexMaster.Chat.Members.Abstractions.DTOs;

namespace HexMaster.Chat.Members.Abstractions.Interfaces;

public interface IMemberRepository
{
    Task<MemberEntityDto?> GetByIdAsync(string id);
    Task<MemberEntityDto> CreateAsync(MemberEntityDto member);
    Task<MemberEntityDto> UpdateAsync(MemberEntityDto member);
    Task DeleteAsync(string id);
    Task<IEnumerable<MemberEntityDto>> GetInactiveMembersAsync(DateTime cutoffTime);
}
