using Dapr.Client;
using HexMaster.Chat.Members.Api.Entities;
using HexMaster.Chat.Members.Api.Repositories;
using HexMaster.Chat.Members.Api.Services;
using HexMaster.Chat.Shared.Constants;
using HexMaster.Chat.Shared.Events;
using HexMaster.Chat.Shared.Requests;
using Microsoft.Extensions.Logging;
using Moq;

namespace HexMaster.Chat.Members.Tests.Services;

public class MemberServiceTests
{
    private readonly Mock<IMemberRepository> _mockRepository;
    private readonly Mock<DaprClient> _mockDaprClient;
    private readonly Mock<ILogger<MemberService>> _mockLogger;
    private readonly MemberService _service;

    public MemberServiceTests()
    {
        _mockRepository = new Mock<IMemberRepository>();
        _mockDaprClient = new Mock<DaprClient>();
        _mockLogger = new Mock<ILogger<MemberService>>();

        _service = new MemberService(_mockRepository.Object, _mockDaprClient.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task RegisterMemberAsync_ShouldCreateMemberAndPublishEvent()
    {
        // Arrange
        var request = new RegisterMemberRequest
        {
            Name = "Test User",
            Email = "test@example.com"
        };

        _mockRepository
            .Setup(x => x.CreateAsync(It.IsAny<MemberEntity>()))
            .ReturnsAsync((MemberEntity entity) => entity);

        _mockDaprClient
            .Setup(x => x.PublishEventAsync(
                DaprComponents.PubSubName,
                Topics.MemberJoined,
                It.IsAny<MemberJoinedEvent>(),
                default))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.RegisterMemberAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test User", result.Name);
        Assert.Equal("test@example.com", result.Email);
        Assert.True(result.IsActive);
        Assert.False(string.IsNullOrEmpty(result.Id));

        _mockRepository.Verify(x => x.CreateAsync(It.Is<MemberEntity>(
            m => m.Name == "Test User" &&
                 m.Email == "test@example.com" &&
                 m.IsActive == true)), Times.Once);

        _mockDaprClient.Verify(x => x.PublishEventAsync(
            DaprComponents.PubSubName,
            Topics.MemberJoined,
            It.Is<MemberJoinedEvent>(e =>
                e.Name == "Test User" &&
                e.Email == "test@example.com"),
            default), Times.Once);
    }

    [Fact]
    public async Task GetMemberAsync_WhenMemberExists_ShouldReturnMember()
    {
        // Arrange
        var memberId = "test-id";
        var memberEntity = new MemberEntity
        {
            RowKey = memberId,
            Name = "Test User",
            Email = "test@example.com",
            JoinedAt = DateTime.UtcNow,
            LastActivityAt = DateTime.UtcNow,
            IsActive = true
        };

        _mockRepository
            .Setup(x => x.GetByIdAsync(memberId))
            .ReturnsAsync(memberEntity);

        // Act
        var result = await _service.GetMemberAsync(memberId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(memberId, result.Id);
        Assert.Equal("Test User", result.Name);
        Assert.Equal("test@example.com", result.Email);
        Assert.True(result.IsActive);
    }

    [Fact]
    public async Task GetMemberAsync_WhenMemberNotFound_ShouldReturnNull()
    {
        // Arrange
        var memberId = "non-existing-id";

        _mockRepository
            .Setup(x => x.GetByIdAsync(memberId))
            .ReturnsAsync((MemberEntity?)null);

        // Act
        var result = await _service.GetMemberAsync(memberId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateLastActivityAsync_WhenMemberExists_ShouldUpdateActivity()
    {
        // Arrange
        var memberId = "test-id";
        var memberEntity = new MemberEntity
        {
            RowKey = memberId,
            Name = "Test User",
            Email = "test@example.com",
            LastActivityAt = DateTime.UtcNow.AddMinutes(-30),
            IsActive = true
        };

        _mockRepository
            .Setup(x => x.GetByIdAsync(memberId))
            .ReturnsAsync(memberEntity);

        _mockRepository
            .Setup(x => x.UpdateAsync(It.IsAny<MemberEntity>()))
            .ReturnsAsync((MemberEntity entity) => entity);

        // Act
        await _service.UpdateLastActivityAsync(memberId);

        // Assert
        _mockRepository.Verify(x => x.UpdateAsync(It.Is<MemberEntity>(
            m => m.RowKey == memberId &&
                 m.LastActivityAt > DateTime.UtcNow.AddMinutes(-1))), Times.Once);
    }

    [Fact]
    public async Task UpdateLastActivityAsync_WhenMemberNotFound_ShouldNotUpdate()
    {
        // Arrange
        var memberId = "non-existing-id";

        _mockRepository
            .Setup(x => x.GetByIdAsync(memberId))
            .ReturnsAsync((MemberEntity?)null);

        // Act
        await _service.UpdateLastActivityAsync(memberId);

        // Assert
        _mockRepository.Verify(x => x.UpdateAsync(It.IsAny<MemberEntity>()), Times.Never);
    }

    [Fact]
    public async Task RemoveInactiveMembersAsync_ShouldRemoveInactiveMembersAndPublishEvents()
    {
        // Arrange
        var cutoffTime = DateTime.UtcNow.AddHours(-1);
        var inactiveMembers = new List<MemberEntity>
        {
            new() { RowKey = "inactive1", Name = "Inactive User 1", LastActivityAt = cutoffTime.AddMinutes(-30) },
            new() { RowKey = "inactive2", Name = "Inactive User 2", LastActivityAt = cutoffTime.AddMinutes(-45) }
        };

        _mockRepository
            .Setup(x => x.GetInactiveMembersAsync(It.IsAny<DateTime>()))
            .ReturnsAsync(inactiveMembers);

        _mockDaprClient
            .Setup(x => x.PublishEventAsync(
                DaprComponents.PubSubName,
                Topics.MemberLeft,
                It.IsAny<MemberLeftEvent>(),
                default))
            .Returns(Task.CompletedTask);

        _mockRepository
            .Setup(x => x.DeleteAsync(It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        // Act
        await _service.RemoveInactiveMembersAsync();

        // Assert
        _mockRepository.Verify(x => x.DeleteAsync("inactive1"), Times.Once);
        _mockRepository.Verify(x => x.DeleteAsync("inactive2"), Times.Once);

        _mockDaprClient.Verify(x => x.PublishEventAsync(
            DaprComponents.PubSubName,
            Topics.MemberLeft,
            It.Is<MemberLeftEvent>(e => e.Id == "inactive1" && e.Name == "Inactive User 1"),
            default), Times.Once);

        _mockDaprClient.Verify(x => x.PublishEventAsync(
            DaprComponents.PubSubName,
            Topics.MemberLeft,
            It.Is<MemberLeftEvent>(e => e.Id == "inactive2" && e.Name == "Inactive User 2"),
            default), Times.Once);
    }

    [Fact]
    public async Task RemoveInactiveMembersAsync_WhenNoInactiveMembers_ShouldNotRemoveAnything()
    {
        // Arrange
        _mockRepository
            .Setup(x => x.GetInactiveMembersAsync(It.IsAny<DateTime>()))
            .ReturnsAsync(new List<MemberEntity>());

        // Act
        await _service.RemoveInactiveMembersAsync();

        // Assert
        _mockRepository.Verify(x => x.DeleteAsync(It.IsAny<string>()), Times.Never);
        _mockDaprClient.Verify(x => x.PublishEventAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<object>(),
            default), Times.Never);
    }
}
