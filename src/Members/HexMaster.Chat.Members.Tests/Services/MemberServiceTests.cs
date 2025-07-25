using Dapr.Client;
using HexMaster.Chat.Members.Abstractions.DTOs;
using HexMaster.Chat.Members.Abstractions.Events;
using HexMaster.Chat.Members.Abstractions.Interfaces;
using HexMaster.Chat.Members.Abstractions.Requests;
using HexMaster.Chat.Members.Services;
using HexMaster.Chat.Shared.Constants;
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
            Name = "Test User"
        };

        _mockRepository
            .Setup(x => x.CreateAsync(It.IsAny<MemberEntityDto>()))
            .ReturnsAsync((MemberEntityDto entity) => entity);

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
        Assert.True(result.IsActive);
        Assert.False(string.IsNullOrEmpty(result.Id));

        _mockRepository.Verify(x => x.CreateAsync(It.Is<MemberEntityDto>(
            m => m.Name == "Test User" &&
                 m.IsActive == true)), Times.Once);

        _mockDaprClient.Verify(x => x.PublishEventAsync(
            DaprComponents.PubSubName,
            Topics.MemberJoined,
            It.Is<MemberJoinedEvent>(e =>
                e.Name == "Test User" &&
                !string.IsNullOrEmpty(e.Id) &&
                e.JoinedAt > DateTime.MinValue),
            default), Times.Once);
    }

    [Fact]
    public async Task GetMemberAsync_WhenMemberExists_ShouldReturnMember()
    {
        // Arrange
        var memberId = "test-id";
        var memberEntityDto = new MemberEntityDto
        {
            RowKey = memberId,
            Name = "Test User",
            JoinedAt = DateTime.UtcNow,
            LastActivityAt = DateTime.UtcNow,
            IsActive = true
        };

        _mockRepository
            .Setup(x => x.GetByIdAsync(memberId))
            .ReturnsAsync(memberEntityDto);

        // Act
        var result = await _service.GetMemberAsync(memberId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(memberId, result.Id);
        Assert.Equal("Test User", result.Name);
        Assert.True(result.IsActive);
    }

    [Fact]
    public async Task GetMemberAsync_WhenMemberNotFound_ShouldReturnNull()
    {
        // Arrange
        var memberId = "non-existing-id";

        _mockRepository
            .Setup(x => x.GetByIdAsync(memberId))
            .ReturnsAsync((MemberEntityDto?)null);

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
        var memberEntity = new MemberEntityDto
        {
            RowKey = memberId,
            Name = "Test User",
            LastActivityAt = DateTime.UtcNow.AddMinutes(-30),
            IsActive = true
        };

        _mockRepository
            .Setup(x => x.GetByIdAsync(memberId))
            .ReturnsAsync(memberEntity);

        _mockRepository
            .Setup(x => x.UpdateAsync(It.IsAny<MemberEntityDto>()))
            .ReturnsAsync((MemberEntityDto entity) => entity);

        // Act
        await _service.UpdateLastActivityAsync(memberId);

        // Assert
        _mockRepository.Verify(x => x.UpdateAsync(It.Is<MemberEntityDto>(
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
            .ReturnsAsync((MemberEntityDto?)null);

        // Act
        await _service.UpdateLastActivityAsync(memberId);

        // Assert
        _mockRepository.Verify(x => x.UpdateAsync(It.IsAny<MemberEntityDto>()), Times.Never);
    }

    [Fact]
    public async Task RemoveInactiveMembersAsync_ShouldRemoveInactiveMembersAndPublishEvents()
    {
        // Arrange
        var cutoffTime = DateTime.UtcNow.AddHours(-1);
        var inactiveMembers = new List<MemberEntityDto>
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
            It.Is<MemberLeftEvent>(e => e.Id == "inactive1" && e.Name == "Inactive User 1" && e.LeftAt > DateTime.MinValue),
            default), Times.Once);

        _mockDaprClient.Verify(x => x.PublishEventAsync(
            DaprComponents.PubSubName,
            Topics.MemberLeft,
            It.Is<MemberLeftEvent>(e => e.Id == "inactive2" && e.Name == "Inactive User 2" && e.LeftAt > DateTime.MinValue),
            default), Times.Once);
    }

    [Fact]
    public async Task RemoveInactiveMembersAsync_WhenNoInactiveMembers_ShouldNotRemoveAnything()
    {
        // Arrange
        _mockRepository
            .Setup(x => x.GetInactiveMembersAsync(It.IsAny<DateTime>()))
            .ReturnsAsync(new List<MemberEntityDto>());

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

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\n")]
    public async Task RegisterMemberAsync_WithInvalidName_ShouldThrowArgumentException(string invalidName)
    {
        // Arrange
        var request = new RegisterMemberRequest
        {
            Name = invalidName
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.RegisterMemberAsync(request));
    }

    [Fact]
    public async Task RegisterMemberAsync_WithNullName_ShouldThrowArgumentException()
    {
        // Arrange
        var request = new RegisterMemberRequest
        {
            Name = null!
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _service.RegisterMemberAsync(request));
    }

    [Fact]
    public async Task RegisterMemberAsync_RepositoryThrowsException_ShouldPropagateException()
    {
        // Arrange
        var request = new RegisterMemberRequest
        {
            Name = "Test User"
        };

        _mockRepository
            .Setup(x => x.CreateAsync(It.IsAny<MemberEntityDto>()))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.RegisterMemberAsync(request));
    }

    [Fact]
    public async Task RegisterMemberAsync_DaprClientThrowsException_ShouldStillReturnMember()
    {
        // Arrange
        var request = new RegisterMemberRequest
        {
            Name = "Test User"
        };

        _mockRepository
            .Setup(x => x.CreateAsync(It.IsAny<MemberEntityDto>()))
            .ReturnsAsync((MemberEntityDto entity) => entity);

        _mockDaprClient
            .Setup(x => x.PublishEventAsync(
                DaprComponents.PubSubName,
                Topics.MemberJoined,
                It.IsAny<MemberJoinedEvent>(),
                default))
            .ThrowsAsync(new InvalidOperationException("Dapr error"));

        // Act & Assert
        // Should not throw exception - event publishing failures should be logged but not fail the operation
        var result = await _service.RegisterMemberAsync(request);
        Assert.NotNull(result);
        Assert.Equal("Test User", result.Name);
    }

    [Fact]
    public async Task GetMemberAsync_WithNullId_ShouldReturnNull()
    {
        // Act
        var result = await _service.GetMemberAsync(null!);

        // Assert
        Assert.Null(result);
        _mockRepository.Verify(x => x.GetByIdAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task GetMemberAsync_WithEmptyId_ShouldReturnNull()
    {
        // Act
        var result = await _service.GetMemberAsync("");

        // Assert
        Assert.Null(result);
        _mockRepository.Verify(x => x.GetByIdAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task UpdateLastActivityAsync_WithNullId_ShouldThrowArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.UpdateLastActivityAsync(null!));
    }

    [Fact]
    public async Task UpdateLastActivityAsync_WithEmptyId_ShouldThrowArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.UpdateLastActivityAsync(""));
    }

    [Fact]
    public async Task UpdateLastActivityAsync_MemberNotFound_ShouldNotUpdate()
    {
        // Arrange
        var memberId = "non-existent-member";
        _mockRepository
            .Setup(x => x.GetByIdAsync(memberId))
            .ReturnsAsync((MemberEntityDto?)null);

        // Act
        await _service.UpdateLastActivityAsync(memberId);

        // Assert - should not attempt to update when member not found
        _mockRepository.Verify(x => x.UpdateAsync(It.IsAny<MemberEntityDto>()), Times.Never);
    }

    [Fact]
    public async Task UpdateLastActivityAsync_RepositoryUpdateFails_ShouldPropagateException()
    {
        // Arrange
        var memberId = "existing-member";
        var existingMember = new MemberEntityDto
        {
            RowKey = memberId,
            Name = "Test User",
            LastActivityAt = DateTime.UtcNow.AddMinutes(-10)
        };

        _mockRepository
            .Setup(x => x.GetByIdAsync(memberId))
            .ReturnsAsync(existingMember);

        _mockRepository
            .Setup(x => x.UpdateAsync(It.IsAny<MemberEntityDto>()))
            .ThrowsAsync(new InvalidOperationException("Update failed"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.UpdateLastActivityAsync(memberId));
    }

    [Fact]
    public async Task RemoveInactiveMembersAsync_RepositoryThrowsException_ShouldPropagateException()
    {
        // Arrange
        _mockRepository
            .Setup(x => x.GetInactiveMembersAsync(It.IsAny<DateTime>()))
            .ThrowsAsync(new InvalidOperationException("Query failed"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.RemoveInactiveMembersAsync());
    }

    [Fact]
    public async Task RemoveInactiveMembersAsync_DeleteFails_ShouldContinueWithOtherMembers()
    {
        // Arrange
        var inactiveMembers = new List<MemberEntityDto>
        {
            new() { RowKey = "member1", Name = "User 1", LastActivityAt = DateTime.UtcNow.AddDays(-8) },
            new() { RowKey = "member2", Name = "User 2", LastActivityAt = DateTime.UtcNow.AddDays(-9) }
        };

        _mockRepository
            .Setup(x => x.GetInactiveMembersAsync(It.IsAny<DateTime>()))
            .ReturnsAsync(inactiveMembers);

        // First delete succeeds, second fails
        _mockRepository
            .Setup(x => x.DeleteAsync("member1"))
            .Returns(Task.CompletedTask);

        _mockRepository
            .Setup(x => x.DeleteAsync("member2"))
            .ThrowsAsync(new InvalidOperationException("Delete failed"));

        _mockDaprClient
            .Setup(x => x.PublishEventAsync(
                DaprComponents.PubSubName,
                Topics.MemberLeft,
                It.IsAny<MemberLeftEvent>(),
                default))
            .Returns(Task.CompletedTask);

        // Act
        await _service.RemoveInactiveMembersAsync();

        // Assert
        _mockRepository.Verify(x => x.DeleteAsync("member1"), Times.Once);
        _mockRepository.Verify(x => x.DeleteAsync("member2"), Times.Once);
        
        // Should publish event for successful deletion only
        _mockDaprClient.Verify(x => x.PublishEventAsync(
            DaprComponents.PubSubName,
            Topics.MemberLeft,
            It.Is<MemberLeftEvent>(e => e.Id == "member1"),
            default), Times.Once);
    }
}
