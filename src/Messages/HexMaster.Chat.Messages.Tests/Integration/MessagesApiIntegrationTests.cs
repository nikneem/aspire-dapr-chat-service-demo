using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Text;
using System.Text.Json;
using Xunit;
using HexMaster.Chat.Messages.Abstractions.Requests;
using HexMaster.Chat.Messages.Abstractions.DTOs;
using HexMaster.Chat.Messages.Abstractions.Interfaces;
using Moq;
using Azure.Data.Tables;
using Dapr.Client;

namespace HexMaster.Chat.Messages.Tests.Integration;

public class MessagesApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };
    
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public MessagesApiIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Remove the real Azure Table Storage dependency
                var tableServiceDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(TableServiceClient));
                if (tableServiceDescriptor != null)
                {
                    services.Remove(tableServiceDescriptor);
                }

                // Remove the real Dapr client
                var daprDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DaprClient));
                if (daprDescriptor != null)
                {
                    services.Remove(daprDescriptor);
                }

                // Remove the real member state service
                var memberStateDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IMemberStateService));
                if (memberStateDescriptor != null)
                {
                    services.Remove(memberStateDescriptor);
                }

                // Add mock dependencies
                var mockTableClient = new Mock<TableClient>();
                var mockTableServiceClient = new Mock<TableServiceClient>();
                mockTableServiceClient
                    .Setup(x => x.GetTableClient(It.IsAny<string>()))
                    .Returns(mockTableClient.Object);
                services.AddSingleton(mockTableServiceClient.Object);

                var mockDaprClient = new Mock<DaprClient>();
                services.AddSingleton(mockDaprClient.Object);
                
                var mockMemberStateService = new Mock<IMemberStateService>();
                mockMemberStateService
                    .Setup(x => x.GetMemberNameAsync(It.IsAny<string>()))
                    .ReturnsAsync("TestUser");
                services.AddSingleton(mockMemberStateService.Object);
            });
        });
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task SendMessage_WithValidRequest_ReturnsCreated()
    {
        // Arrange
        var request = new SendMessageRequest
        {
            Content = "Hello, World!",
            SenderId = Guid.NewGuid().ToString()
        };
        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/messages", content);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var location = response.Headers.Location?.ToString();
        Assert.NotNull(location);
        Assert.StartsWith("/messages/", location);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var message = JsonSerializer.Deserialize<ChatMessageDto>(responseContent, JsonOptions);
        
        Assert.NotNull(message);
        Assert.Equal("Hello, World!", message.Content);
        Assert.Equal(request.SenderId, message.SenderId);
        Assert.NotNull(message.Id);
    }

    [Fact]
    public async Task SendMessage_WithEmptyContent_ReturnsBadRequest()
    {
        // Arrange
        var request = new SendMessageRequest
        {
            Content = "",
            SenderId = Guid.NewGuid().ToString()
        };
        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/messages", content);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task SendMessage_WithNullContent_ReturnsBadRequest()
    {
        // Arrange
        var request = new SendMessageRequest
        {
            Content = null!,
            SenderId = Guid.NewGuid().ToString()
        };
        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/messages", content);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task SendMessage_WithEmptySenderId_ReturnsBadRequest()
    {
        // Arrange
        var request = new SendMessageRequest
        {
            Content = "Hello, World!",
            SenderId = ""
        };
        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/messages", content);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task SendMessage_WithNullSenderId_ReturnsBadRequest()
    {
        // Arrange
        var request = new SendMessageRequest
        {
            Content = "Hello, World!",
            SenderId = null!
        };
        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/messages", content);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task SendMessage_WithWhitespaceContent_ReturnsBadRequest()
    {
        // Arrange
        var request = new SendMessageRequest
        {
            Content = "   ",
            SenderId = Guid.NewGuid().ToString()
        };
        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/messages", content);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task SendMessage_WithWhitespaceSenderId_ReturnsBadRequest()
    {
        // Arrange
        var request = new SendMessageRequest
        {
            Content = "Hello, World!",
            SenderId = "   "
        };
        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/messages", content);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task SendMessage_ServiceThrowsArgumentException_ReturnsBadRequest()
    {
        // Arrange
        var request = new SendMessageRequest
        {
            Content = "Hello, World!",
            SenderId = Guid.NewGuid().ToString()
        };

        var factory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Replace the message service with a mock
                var serviceDescriptor = services.Single(d => d.ServiceType == typeof(IMessageService));
                services.Remove(serviceDescriptor);

                var mockMessageService = new Mock<IMessageService>();
                mockMessageService.Setup(s => s.SendMessageAsync(It.IsAny<SendMessageRequest>()))
                    .ThrowsAsync(new ArgumentException("Invalid sender"));
                services.AddSingleton(mockMessageService.Object);
            });
        });

        using var client = factory.CreateClient();
        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await client.PostAsync("/messages", content);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetRecentMessages_WithDefaultCount_ReturnsOk()
    {
        // Arrange
        var expectedMessages = new List<ChatMessageDto>
        {
            new() { Id = Guid.NewGuid().ToString(), Content = "Message 1", SenderId = "user1", SentAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid().ToString(), Content = "Message 2", SenderId = "user2", SentAt = DateTime.UtcNow }
        };

        var factory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Replace the message service with a mock
                var serviceDescriptor = services.Single(d => d.ServiceType == typeof(IMessageService));
                services.Remove(serviceDescriptor);

                var mockMessageService = new Mock<IMessageService>();
                mockMessageService.Setup(s => s.GetRecentMessagesAsync(50))
                    .ReturnsAsync(expectedMessages);
                services.AddSingleton(mockMessageService.Object);
            });
        });

        using var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync("/messages/recent");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var responseContent = await response.Content.ReadAsStringAsync();
        var messages = JsonSerializer.Deserialize<List<ChatMessageDto>>(responseContent, JsonOptions);
        
        Assert.NotNull(messages);
        Assert.Equal(2, messages.Count);
        Assert.Equal("Message 1", messages[0].Content);
        Assert.Equal("Message 2", messages[1].Content);
    }

    [Fact]
    public async Task GetRecentMessages_WithCustomCount_ReturnsOk()
    {
        // Arrange
        var expectedMessages = new List<ChatMessageDto>
        {
            new() { Id = Guid.NewGuid().ToString(), Content = "Message 1", SenderId = "user1", SentAt = DateTime.UtcNow }
        };

        var factory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Replace the message service with a mock
                var serviceDescriptor = services.Single(d => d.ServiceType == typeof(IMessageService));
                services.Remove(serviceDescriptor);

                var mockMessageService = new Mock<IMessageService>();
                mockMessageService.Setup(s => s.GetRecentMessagesAsync(10))
                    .ReturnsAsync(expectedMessages);
                services.AddSingleton(mockMessageService.Object);
            });
        });

        using var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync("/messages/recent?count=10");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetMessageHistory_WithDefaultDates_ReturnsOk()
    {
        // Arrange
        var expectedMessages = new List<ChatMessageDto>
        {
            new() { Id = Guid.NewGuid().ToString(), Content = "History Message", SenderId = "user1", SentAt = DateTime.UtcNow }
        };

        var factory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Replace the message service with a mock
                var serviceDescriptor = services.Single(d => d.ServiceType == typeof(IMessageService));
                services.Remove(serviceDescriptor);

                var mockMessageService = new Mock<IMessageService>();
                mockMessageService.Setup(s => s.GetMessageHistoryAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                    .ReturnsAsync(expectedMessages);
                services.AddSingleton(mockMessageService.Object);
            });
        });

        using var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync("/messages/history");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var responseContent = await response.Content.ReadAsStringAsync();
        var messages = JsonSerializer.Deserialize<List<ChatMessageDto>>(responseContent, JsonOptions);
        
        Assert.NotNull(messages);
        Assert.Single(messages);
        Assert.Equal("History Message", messages[0].Content);
    }

    [Fact]
    public async Task GetMessageHistory_WithCustomDates_ReturnsOk()
    {
        // Arrange
        var from = DateTime.UtcNow.AddHours(-2);
        var to = DateTime.UtcNow.AddHours(-1);
        var expectedMessages = new List<ChatMessageDto>
        {
            new() { Id = Guid.NewGuid().ToString(), Content = "History Message", SenderId = "user1", SentAt = from.AddMinutes(30) }
        };

        var factory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Replace the message service with a mock
                var serviceDescriptor = services.Single(d => d.ServiceType == typeof(IMessageService));
                services.Remove(serviceDescriptor);

                var mockMessageService = new Mock<IMessageService>();
                mockMessageService.Setup(s => s.GetMessageHistoryAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                    .ReturnsAsync(expectedMessages);
                services.AddSingleton(mockMessageService.Object);
            });
        });

        using var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync($"/messages/history?from={from:yyyy-MM-ddTHH:mm:ssZ}&to={to:yyyy-MM-ddTHH:mm:ssZ}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task SendMessage_WithInvalidJson_ReturnsBadRequest()
    {
        // Arrange
        var content = new StringContent("invalid json", Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/messages", content);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task SendMessage_WithMissingContentType_ReturnsBadRequest()
    {
        // Arrange
        var request = new SendMessageRequest
        {
            Content = "Hello, World!",
            SenderId = Guid.NewGuid().ToString()
        };
        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8);

        // Act
        var response = await _client.PostAsync("/messages", content);

        // Assert
        Assert.Equal(HttpStatusCode.UnsupportedMediaType, response.StatusCode);
    }
}
