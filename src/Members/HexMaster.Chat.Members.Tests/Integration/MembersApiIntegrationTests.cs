using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Text;
using System.Text.Json;
using Xunit;
using HexMaster.Chat.Members.Abstractions.Requests;
using HexMaster.Chat.Members.Abstractions.DTOs;
using HexMaster.Chat.Members.Abstractions.Interfaces;
using Moq;
using Azure.Data.Tables;
using Dapr.Client;

namespace HexMaster.Chat.Members.Tests.Integration;

public class MembersApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public MembersApiIntegrationTests(WebApplicationFactory<Program> factory)
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

                // Add mock dependencies
                var mockTableClient = new Mock<TableClient>();
                var mockTableServiceClient = new Mock<TableServiceClient>();
                mockTableServiceClient
                    .Setup(x => x.GetTableClient(It.IsAny<string>()))
                    .Returns(mockTableClient.Object);
                services.AddSingleton(mockTableServiceClient.Object);

                var mockDaprClient = new Mock<DaprClient>();
                services.AddSingleton(mockDaprClient.Object);
            });
        });
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task RegisterMember_WithValidRequest_ReturnsCreated()
    {
        // Arrange
        var request = new RegisterMemberRequest
        {
            Name = "Test User"
        };
        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/members", content);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var location = response.Headers.Location?.ToString();
        Assert.NotNull(location);
        Assert.StartsWith("/members/", location);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var member = JsonSerializer.Deserialize<ChatMemberDto>(responseContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        
        Assert.NotNull(member);
        Assert.Equal("Test User", member.Name);
        Assert.NotNull(member.Id);
    }

    [Fact]
    public async Task RegisterMember_WithEmptyName_ReturnsBadRequest()
    {
        // Arrange
        var request = new RegisterMemberRequest
        {
            Name = ""
        };
        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/members", content);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task RegisterMember_WithNullName_ReturnsBadRequest()
    {
        // Arrange
        var request = new RegisterMemberRequest
        {
            Name = null!
        };
        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/members", content);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task RegisterMember_WithWhitespaceName_ReturnsBadRequest()
    {
        // Arrange
        var request = new RegisterMemberRequest
        {
            Name = "   "
        };
        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/members", content);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetMember_WithValidId_ReturnsOk()
    {
        // Arrange
        var memberId = Guid.NewGuid().ToString();
        var expectedMember = new ChatMemberDto
        {
            Id = memberId,
            Name = "Test User",
            LastActivityAt = DateTime.UtcNow
        };

        var factory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Replace the member service with a mock
                var serviceDescriptor = services.Single(d => d.ServiceType == typeof(IMemberService));
                services.Remove(serviceDescriptor);

                var mockMemberService = new Mock<IMemberService>();
                mockMemberService.Setup(s => s.GetMemberAsync(memberId))
                    .ReturnsAsync(expectedMember);
                services.AddSingleton(mockMemberService.Object);
            });
        });

        using var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync($"/members/{memberId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var responseContent = await response.Content.ReadAsStringAsync();
        var member = JsonSerializer.Deserialize<ChatMemberDto>(responseContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        
        Assert.NotNull(member);
        Assert.Equal(expectedMember.Id, member.Id);
        Assert.Equal(expectedMember.Name, member.Name);
    }

    [Fact]
    public async Task GetMember_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var memberId = Guid.NewGuid().ToString();

        var factory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Replace the member service with a mock
                var serviceDescriptor = services.Single(d => d.ServiceType == typeof(IMemberService));
                services.Remove(serviceDescriptor);

                var mockMemberService = new Mock<IMemberService>();
                mockMemberService.Setup(s => s.GetMemberAsync(memberId))
                    .ReturnsAsync((ChatMemberDto?)null);
                services.AddSingleton(mockMemberService.Object);
            });
        });

        using var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync($"/members/{memberId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateMemberActivity_WithValidId_ReturnsOk()
    {
        // Arrange
        var memberId = Guid.NewGuid().ToString();

        var factory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Replace the member service with a mock
                var serviceDescriptor = services.Single(d => d.ServiceType == typeof(IMemberService));
                services.Remove(serviceDescriptor);

                var mockMemberService = new Mock<IMemberService>();
                mockMemberService.Setup(s => s.UpdateLastActivityAsync(memberId))
                    .Returns(Task.CompletedTask);
                services.AddSingleton(mockMemberService.Object);
            });
        });

        using var client = factory.CreateClient();

        // Act
        var response = await client.PutAsync($"/members/{memberId}/activity", null);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task RegisterMember_WithInvalidJson_ReturnsBadRequest()
    {
        // Arrange
        var content = new StringContent("invalid json", Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/members", content);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task RegisterMember_WithMissingContentType_ReturnsBadRequest()
    {
        // Arrange
        var request = new RegisterMemberRequest
        {
            Name = "Test User"
        };
        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8);

        // Act
        var response = await _client.PostAsync("/members", content);

        // Assert
        Assert.Equal(HttpStatusCode.UnsupportedMediaType, response.StatusCode);
    }
}
