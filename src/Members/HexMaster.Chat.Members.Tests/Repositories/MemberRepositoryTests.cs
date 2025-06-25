using Azure;
using Azure.Data.Tables;
using HexMaster.Chat.Members.Abstractions.DTOs;
using HexMaster.Chat.Members.Entities;
using HexMaster.Chat.Members.Repositories;
using Moq;

namespace HexMaster.Chat.Members.Tests.Repositories;

public class MemberRepositoryTests
{
    private readonly Mock<TableServiceClient> _mockTableServiceClient;
    private readonly Mock<TableClient> _mockTableClient;
    private readonly MemberRepository _repository;

    public MemberRepositoryTests()
    {
        _mockTableServiceClient = new Mock<TableServiceClient>();
        _mockTableClient = new Mock<TableClient>();

        _mockTableServiceClient
            .Setup(x => x.GetTableClient(It.IsAny<string>()))
            .Returns(_mockTableClient.Object);

        _repository = new MemberRepository(_mockTableServiceClient.Object);
    }

    [Fact]
    public async Task GetByIdAsync_WhenMemberExists_ShouldReturnMember()
    {
        // Arrange
        var memberId = "test-id";
        var memberEntity = new MemberEntity
        {
            RowKey = memberId,
            Name = "Test User",
            Email = "test@example.com"
        };

        var response = Response.FromValue(memberEntity, Mock.Of<Response>());
        _mockTableClient
            .Setup(x => x.GetEntityAsync<MemberEntity>("member", memberId, null, default))
            .ReturnsAsync(response);

        // Act
        var result = await _repository.GetByIdAsync(memberId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(memberId, result.RowKey);
        Assert.Equal("Test User", result.Name);
        Assert.Equal("test@example.com", result.Email);
    }

    [Fact]
    public async Task GetByIdAsync_WhenMemberNotFound_ShouldReturnNull()
    {
        // Arrange
        var memberId = "non-existing-id";
        var exception = new RequestFailedException(404, "Not found");

        _mockTableClient
            .Setup(x => x.GetEntityAsync<MemberEntity>("member", memberId, null, default))
            .ThrowsAsync(exception);

        // Act
        var result = await _repository.GetByIdAsync(memberId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAsync_ShouldCallAddEntityAndReturnMember()
    {
        // Arrange
        var memberEntityDto = new MemberEntityDto
        {
            RowKey = "test-id",
            Name = "Test User",
            Email = "test@example.com"
        };

        var response = Mock.Of<Response>();
        _mockTableClient
            .Setup(x => x.AddEntityAsync(It.IsAny<MemberEntity>(), default))
            .ReturnsAsync(response);

        // Act
        var result = await _repository.CreateAsync(memberEntityDto);

        // Assert
        Assert.Equal(memberEntityDto.RowKey, result.RowKey);
        Assert.Equal(memberEntityDto.Name, result.Name);
        Assert.Equal(memberEntityDto.Email, result.Email);
        _mockTableClient.Verify(x => x.AddEntityAsync(It.IsAny<MemberEntity>(), default), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ShouldCallUpdateEntityAndReturnMember()
    {
        // Arrange
        var memberEntityDto = new MemberEntityDto
        {
            RowKey = "test-id",
            Name = "Updated User",
            Email = "updated@example.com",
            ETag = new ETag("test-etag")
        };

        var response = Mock.Of<Response>();
        _mockTableClient
            .Setup(x => x.UpdateEntityAsync(It.IsAny<MemberEntity>(), It.IsAny<ETag>(), TableUpdateMode.Merge, default))
            .ReturnsAsync(response);

        // Act
        var result = await _repository.UpdateAsync(memberEntityDto);

        // Assert
        Assert.Equal(memberEntityDto.RowKey, result.RowKey);
        Assert.Equal(memberEntityDto.Name, result.Name);
        Assert.Equal(memberEntityDto.Email, result.Email);
        _mockTableClient.Verify(x => x.UpdateEntityAsync(It.IsAny<MemberEntity>(), It.IsAny<ETag>(), TableUpdateMode.Merge, default), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ShouldCallDeleteEntity()
    {
        // Arrange
        var memberId = "test-id";
        var response = Mock.Of<Response>();

        _mockTableClient
            .Setup(x => x.DeleteEntityAsync("member", memberId, ETag.All, default))
            .ReturnsAsync(response);

        // Act
        await _repository.DeleteAsync(memberId);

        // Assert
        _mockTableClient.Verify(x => x.DeleteEntityAsync("member", memberId, It.IsAny<ETag>(), default), Times.Once);
    }

    [Fact]
    public async Task GetInactiveMembersAsync_ShouldReturnInactiveMembers()
    {
        // Arrange
        var cutoffTime = DateTime.UtcNow.AddHours(-1);
        var inactiveMembers = new List<MemberEntity>
        {
            new() { RowKey = "inactive1", Name = "Inactive User 1", LastActivityAt = cutoffTime.AddMinutes(-30) },
            new() { RowKey = "inactive2", Name = "Inactive User 2", LastActivityAt = cutoffTime.AddMinutes(-45) }
        };

        var mockAsyncPageable = new Mock<AsyncPageable<MemberEntity>>();
        mockAsyncPageable
            .Setup(x => x.GetAsyncEnumerator(default))
            .Returns(CreateAsyncEnumerator(inactiveMembers));

        _mockTableClient
            .Setup(x => x.QueryAsync<MemberEntity>(It.IsAny<string>(), null, null, default))
            .Returns(mockAsyncPageable.Object);

        // Act
        var result = await _repository.GetInactiveMembersAsync(cutoffTime);

        // Assert
        Assert.Equal(2, result.Count());
        Assert.Contains(result, m => m.RowKey == "inactive1");
        Assert.Contains(result, m => m.RowKey == "inactive2");
    }

    private static async IAsyncEnumerator<MemberEntity> CreateAsyncEnumerator(IEnumerable<MemberEntity> items)
    {
        foreach (var item in items)
        {
            yield return item;
        }
        await Task.CompletedTask;
    }
}
