using Azure;
using Azure.Data.Tables;
using HexMaster.Chat.Members.Api.Entities;
using HexMaster.Chat.Shared.Constants;

namespace HexMaster.Chat.Members.Api.Repositories;

public interface IMemberRepository
{
    Task<MemberEntity?> GetByIdAsync(string id);
    Task<MemberEntity> CreateAsync(MemberEntity member);
    Task<MemberEntity> UpdateAsync(MemberEntity member);
    Task DeleteAsync(string id);
    Task<IEnumerable<MemberEntity>> GetInactiveMembersAsync(DateTime cutoffTime);
}

public class MemberRepository : IMemberRepository
{
    private readonly TableClient _tableClient;

    public MemberRepository(TableServiceClient tableServiceClient)
    {
        _tableClient = tableServiceClient.GetTableClient(TableNames.Members);
        _tableClient.CreateIfNotExists();
    }

    public async Task<MemberEntity?> GetByIdAsync(string id)
    {
        try
        {
            var response = await _tableClient.GetEntityAsync<MemberEntity>("member", id);
            return response.Value;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            return null;
        }
    }

    public async Task<MemberEntity> CreateAsync(MemberEntity member)
    {
        await _tableClient.AddEntityAsync(member);
        return member;
    }

    public async Task<MemberEntity> UpdateAsync(MemberEntity member)
    {
        await _tableClient.UpdateEntityAsync(member, member.ETag);
        return member;
    }

    public async Task DeleteAsync(string id)
    {
        await _tableClient.DeleteEntityAsync("member", id);
    }

    public async Task<IEnumerable<MemberEntity>> GetInactiveMembersAsync(DateTime cutoffTime)
    {
        var filter = $"LastActivityAt lt datetime'{cutoffTime:yyyy-MM-ddTHH:mm:ssZ}'";
        var entities = new List<MemberEntity>();

        await foreach (var entity in _tableClient.QueryAsync<MemberEntity>(filter))
        {
            entities.Add(entity);
        }

        return entities;
    }
}
