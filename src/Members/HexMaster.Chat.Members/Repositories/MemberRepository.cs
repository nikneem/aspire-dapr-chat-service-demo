using Azure;
using Azure.Data.Tables;
using HexMaster.Chat.Members.Abstractions.DTOs;
using HexMaster.Chat.Members.Abstractions.Interfaces;
using HexMaster.Chat.Members.Entities;
using HexMaster.Chat.Shared.Constants;

namespace HexMaster.Chat.Members.Repositories;

public class MemberRepository : IMemberRepository
{
    private readonly TableClient _tableClient;

    public MemberRepository(TableServiceClient tableServiceClient)
    {
        _tableClient = tableServiceClient.GetTableClient(TableNames.Members);
        _tableClient.CreateIfNotExists();
    }

    public async Task<MemberEntityDto?> GetByIdAsync(string id)
    {
        try
        {
            var response = await _tableClient.GetEntityAsync<MemberEntity>("member", id);
            return MapToDto(response.Value);
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            return null;
        }
    }

    public async Task<MemberEntityDto> CreateAsync(MemberEntityDto memberDto)
    {
        var entity = MapToEntity(memberDto);
        await _tableClient.AddEntityAsync(entity);
        return MapToDto(entity);
    }

    public async Task<MemberEntityDto> UpdateAsync(MemberEntityDto memberDto)
    {
        var entity = MapToEntity(memberDto);
        await _tableClient.UpdateEntityAsync(entity, entity.ETag);
        return MapToDto(entity);
    }

    public async Task DeleteAsync(string id)
    {
        await _tableClient.DeleteEntityAsync("member", id);
    }

    public async Task<IEnumerable<MemberEntityDto>> GetInactiveMembersAsync(DateTime cutoffTime)
    {
        var filter = $"LastActivityAt lt datetime'{cutoffTime:yyyy-MM-ddTHH:mm:ssZ}'";
        var entities = new List<MemberEntityDto>();

        await foreach (var entity in _tableClient.QueryAsync<MemberEntity>(filter))
        {
            entities.Add(MapToDto(entity));
        }

        return entities;
    }

    private static MemberEntityDto MapToDto(MemberEntity entity)
    {
        return new MemberEntityDto
        {
            PartitionKey = entity.PartitionKey,
            RowKey = entity.RowKey,
            Timestamp = entity.Timestamp,
            ETag = entity.ETag,
            Name = entity.Name,
            Email = entity.Email,
            JoinedAt = entity.JoinedAt,
            LastActivityAt = entity.LastActivityAt,
            IsActive = entity.IsActive
        };
    }

    private static MemberEntity MapToEntity(MemberEntityDto dto)
    {
        return new MemberEntity
        {
            PartitionKey = dto.PartitionKey,
            RowKey = dto.RowKey,
            Timestamp = dto.Timestamp,
            ETag = dto.ETag,
            Name = dto.Name,
            Email = dto.Email,
            JoinedAt = dto.JoinedAt,
            LastActivityAt = dto.LastActivityAt,
            IsActive = dto.IsActive
        };
    }
}
