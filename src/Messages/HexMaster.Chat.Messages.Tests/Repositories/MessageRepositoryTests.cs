using Azure;
using Azure.Data.Tables;
using HexMaster.Chat.Messages.Api.Entities;
using HexMaster.Chat.Messages.Api.Repositories;
using Moq;

namespace HexMaster.Chat.Messages.Tests.Repositories;

public class MessageRepositoryTests
{
    private readonly Mock<TableServiceClient> _mockTableServiceClient;
    private readonly Mock<TableClient> _mockTableClient;
    private readonly MessageRepository _repository;

    public MessageRepositoryTests()
    {
        _mockTableServiceClient = new Mock<TableServiceClient>();
        _mockTableClient = new Mock<TableClient>();

        _mockTableServiceClient
            .Setup(x => x.GetTableClient(It.IsAny<string>()))
            .Returns(_mockTableClient.Object);

        _repository = new MessageRepository(_mockTableServiceClient.Object);
    }

    [Fact]
    public async Task CreateAsync_ShouldCallAddEntityAndReturnMessage()
    {
        // Arrange
        var messageEntity = new MessageEntity
        {
            RowKey = "msg-123",
            Content = "Test message",
            SenderId = "sender-456",
            SenderName = "Test Sender"
        };

        var response = Mock.Of<Response>();
        _mockTableClient
            .Setup(x => x.AddEntityAsync(messageEntity, default))
            .ReturnsAsync(response);

        // Act
        var result = await _repository.CreateAsync(messageEntity);

        // Assert
        Assert.Equal(messageEntity, result);
        _mockTableClient.Verify(x => x.AddEntityAsync(messageEntity, default), Times.Once);
    }

    [Fact]
    public async Task GetRecentMessagesAsync_ShouldReturnOrderedMessages()
    {
        // Arrange
        var messages = new List<MessageEntity>
        {
            new()
            {
                RowKey = "msg1",
                Content = "First message",
                SentAt = DateTime.UtcNow.AddMinutes(-10)
            },
            new()
            {
                RowKey = "msg2",
                Content = "Second message",
                SentAt = DateTime.UtcNow.AddMinutes(-5)
            },
            new()
            {
                RowKey = "msg3",
                Content = "Third message",
                SentAt = DateTime.UtcNow.AddMinutes(-1)
            }
        };

        var mockAsyncPageable = new Mock<AsyncPageable<MessageEntity>>();
        mockAsyncPageable
            .Setup(x => x.GetAsyncEnumerator(default))
            .Returns(CreateAsyncEnumerator(messages));

        _mockTableClient
            .Setup(x => x.QueryAsync<MessageEntity>(
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<IEnumerable<string>>(),
                default))
            .Returns(mockAsyncPageable.Object);

        // Act
        var result = await _repository.GetRecentMessagesAsync(50);

        // Assert
        var resultList = result.ToList();
        Assert.Equal(3, resultList.Count);

        // Should be ordered by SentAt descending (most recent first)
        Assert.Equal("msg3", resultList[0].RowKey);
        Assert.Equal("msg2", resultList[1].RowKey);
        Assert.Equal("msg1", resultList[2].RowKey);
    }

    [Fact]
    public async Task GetRecentMessagesAsync_WithCustomCount_ShouldLimitResults()
    {
        // Arrange
        var messages = new List<MessageEntity>
        {
            new() { RowKey = "msg1", SentAt = DateTime.UtcNow.AddMinutes(-10) },
            new() { RowKey = "msg2", SentAt = DateTime.UtcNow.AddMinutes(-5) },
            new() { RowKey = "msg3", SentAt = DateTime.UtcNow.AddMinutes(-1) }
        };

        var mockAsyncPageable = new Mock<AsyncPageable<MessageEntity>>();
        mockAsyncPageable
            .Setup(x => x.GetAsyncEnumerator(default))
            .Returns(CreateAsyncEnumerator(messages));

        _mockTableClient
            .Setup(x => x.QueryAsync<MessageEntity>(
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<IEnumerable<string>>(),
                default))
            .Returns(mockAsyncPageable.Object);

        // Act
        var result = await _repository.GetRecentMessagesAsync(2);

        // Assert
        var resultList = result.ToList();
        Assert.Equal(2, resultList.Count);
    }

    [Fact]
    public async Task GetMessagesByDateRangeAsync_ShouldReturnFilteredMessages()
    {
        // Arrange
        var from = DateTime.UtcNow.AddHours(-2);
        var to = DateTime.UtcNow;
        var messages = new List<MessageEntity>
        {
            new()
            {
                RowKey = "msg1",
                Content = "Message 1",
                SentAt = from.AddMinutes(30)
            },
            new()
            {
                RowKey = "msg2",
                Content = "Message 2",
                SentAt = from.AddMinutes(60)
            }
        };

        var mockAsyncPageable = new Mock<AsyncPageable<MessageEntity>>();
        mockAsyncPageable
            .Setup(x => x.GetAsyncEnumerator(default))
            .Returns(CreateAsyncEnumerator(messages));

        _mockTableClient
            .Setup(x => x.QueryAsync<MessageEntity>(It.IsAny<string>(), null, null, default))
            .Returns(mockAsyncPageable.Object);

        // Act
        var result = await _repository.GetMessagesByDateRangeAsync(from, to);

        // Assert
        var resultList = result.ToList();
        Assert.Equal(2, resultList.Count);

        // Should be ordered by SentAt ascending
        Assert.Equal("msg1", resultList[0].RowKey);
        Assert.Equal("msg2", resultList[1].RowKey);
    }

    [Fact]
    public async Task GetExpiredMessagesAsync_ShouldReturnExpiredMessages()
    {
        // Arrange
        var cutoffTime = DateTime.UtcNow.AddHours(-24);
        var expiredMessages = new List<MessageEntity>
        {
            new()
            {
                RowKey = "expired1",
                Content = "Expired message 1",
                SentAt = cutoffTime.AddHours(-1)
            },
            new()
            {
                RowKey = "expired2",
                Content = "Expired message 2",
                SentAt = cutoffTime.AddHours(-2)
            }
        };

        var mockAsyncPageable = new Mock<AsyncPageable<MessageEntity>>();
        mockAsyncPageable
            .Setup(x => x.GetAsyncEnumerator(default))
            .Returns(CreateAsyncEnumerator(expiredMessages));

        _mockTableClient
            .Setup(x => x.QueryAsync<MessageEntity>(It.IsAny<string>(), null, null, default))
            .Returns(mockAsyncPageable.Object);

        // Act
        var result = await _repository.GetExpiredMessagesAsync(cutoffTime);

        // Assert
        Assert.Equal(2, result.Count());
        Assert.Contains(result, m => m.RowKey == "expired1");
        Assert.Contains(result, m => m.RowKey == "expired2");
    }

    [Fact]
    public async Task DeleteAsync_ShouldCallDeleteEntity()
    {
        // Arrange
        var messageId = "msg-123";
        var response = Mock.Of<Response>();

        _mockTableClient
            .Setup(x => x.DeleteEntityAsync("message", messageId, It.IsAny<ETag>(), default))
            .ReturnsAsync(response);

        // Act
        await _repository.DeleteAsync(messageId);

        // Assert
        _mockTableClient.Verify(x => x.DeleteEntityAsync("message", messageId, It.IsAny<ETag>(), default), Times.Once);
    }

    [Fact]
    public async Task DeleteBatchAsync_ShouldSubmitBatchTransactions()
    {
        // Arrange
        var messageIds = new[] { "msg1", "msg2", "msg3" };
        var response = Mock.Of<Response<IReadOnlyList<Response>>>();

        _mockTableClient
            .Setup(x => x.SubmitTransactionAsync(It.IsAny<IEnumerable<TableTransactionAction>>(), default))
            .ReturnsAsync(response);

        // Act
        await _repository.DeleteBatchAsync(messageIds);

        // Assert
        _mockTableClient.Verify(x => x.SubmitTransactionAsync(
            It.Is<IEnumerable<TableTransactionAction>>(actions => actions.Count() == 3),
            default), Times.Once);
    }

    [Fact]
    public async Task DeleteBatchAsync_WithLargeBatch_ShouldChunkRequests()
    {
        // Arrange
        var messageIds = Enumerable.Range(1, 250).Select(i => $"msg{i}").ToArray();
        var response = Mock.Of<Response<IReadOnlyList<Response>>>();

        _mockTableClient
            .Setup(x => x.SubmitTransactionAsync(It.IsAny<IEnumerable<TableTransactionAction>>(), default))
            .ReturnsAsync(response);

        // Act
        await _repository.DeleteBatchAsync(messageIds);

        // Assert
        // Should be called 3 times: 100 + 100 + 50
        _mockTableClient.Verify(x => x.SubmitTransactionAsync(
            It.IsAny<IEnumerable<TableTransactionAction>>(),
            default), Times.Exactly(3));
    }

    [Fact]
    public async Task DeleteBatchAsync_WhenTransactionFails_ShouldContinueWithNextBatch()
    {
        // Arrange
        var messageIds = new[] { "msg1", "msg2", "msg3", "msg4" };
        var response = Mock.Of<Response<IReadOnlyList<Response>>>();
        var exception = new RequestFailedException(400, "Transaction failed");

        _mockTableClient
            .SetupSequence(x => x.SubmitTransactionAsync(It.IsAny<IEnumerable<TableTransactionAction>>(), default))
            .ThrowsAsync(exception)
            .ReturnsAsync(response);

        // Act & Assert
        // Should not throw exception, should continue with remaining batches
        await _repository.DeleteBatchAsync(messageIds);

        _mockTableClient.Verify(x => x.SubmitTransactionAsync(
            It.IsAny<IEnumerable<TableTransactionAction>>(),
            default), Times.Once);
    }

    private static async IAsyncEnumerator<MessageEntity> CreateAsyncEnumerator(IEnumerable<MessageEntity> items)
    {
        foreach (var item in items)
        {
            yield return item;
        }
        await Task.CompletedTask;
    }
}
