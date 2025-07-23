using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.SignalR.Client;
using System.Net;
using Xunit;

namespace HexMaster.Chat.Realtime.Tests.Integration;

public class RealtimeApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public RealtimeApiIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task ChatHub_CanConnect()
    {
        // Arrange
        var hubUrl = $"{_client.BaseAddress}chathub";
        
        // Act & Assert
        var connection = new HubConnectionBuilder()
            .WithUrl(hubUrl, options =>
            {
                options.HttpMessageHandlerFactory = _ => _factory.Server.CreateHandler();
            })
            .Build();

        await connection.StartAsync();
        Assert.Equal(HubConnectionState.Connected, connection.State);
        
        await connection.DisposeAsync();
    }

    [Fact]
    public async Task ChatHub_CanSendMessage()
    {
        // Arrange
        var hubUrl = $"{_client.BaseAddress}chathub";
        var messageReceived = false;
        var receivedMessage = string.Empty;
        var receivedSender = string.Empty;

        var connection = new HubConnectionBuilder()
            .WithUrl(hubUrl, options =>
            {
                options.HttpMessageHandlerFactory = _ => _factory.Server.CreateHandler();
            })
            .Build();

        connection.On<string, string, string, string, DateTime>("ReceiveMessage", (id, content, senderId, senderName, sentAt) =>
        {
            receivedSender = senderName;
            receivedMessage = content;
            messageReceived = true;
        });

        await connection.StartAsync();

        // Act
        await connection.InvokeAsync("SendMessage", "TestUser", "Hello, World!");

        // Wait for message to be received
        var timeout = DateTime.UtcNow.AddSeconds(5);
        while (!messageReceived && DateTime.UtcNow < timeout)
        {
            await Task.Delay(50);
        }

        // Assert
        Assert.True(messageReceived);
        Assert.Equal("TestUser", receivedSender);
        Assert.Equal("Hello, World!", receivedMessage);
        
        await connection.DisposeAsync();
    }

    [Fact]
    public async Task ChatHub_CanJoinGroup()
    {
        // Arrange
        var hubUrl = $"{_client.BaseAddress}chathub";
        var connection = new HubConnectionBuilder()
            .WithUrl(hubUrl, options =>
            {
                options.HttpMessageHandlerFactory = _ => _factory.Server.CreateHandler();
            })
            .Build();

        await connection.StartAsync();

        // Act & Assert - should not throw
        await connection.InvokeAsync("JoinGroup", "TestGroup");
        
        await connection.DisposeAsync();
    }

    [Fact]
    public async Task ChatHub_CanLeaveGroup()
    {
        // Arrange
        var hubUrl = $"{_client.BaseAddress}chathub";
        var connection = new HubConnectionBuilder()
            .WithUrl(hubUrl, options =>
            {
                options.HttpMessageHandlerFactory = _ => _factory.Server.CreateHandler();
            })
            .Build();

        await connection.StartAsync();

        // Act & Assert - should not throw
        await connection.InvokeAsync("JoinGroup", "TestGroup");
        await connection.InvokeAsync("LeaveGroup", "TestGroup");
        
        await connection.DisposeAsync();
    }

    [Fact]
    public async Task ChatHub_MultipleConnections_CanCommunicate()
    {
        // Arrange
        var hubUrl = $"{_client.BaseAddress}chathub";
        var connection1MessageReceived = false;
        var connection2MessageReceived = false;

        var connection1 = new HubConnectionBuilder()
            .WithUrl(hubUrl, options =>
            {
                options.HttpMessageHandlerFactory = _ => _factory.Server.CreateHandler();
            })
            .Build();

        var connection2 = new HubConnectionBuilder()
            .WithUrl(hubUrl, options =>
            {
                options.HttpMessageHandlerFactory = _ => _factory.Server.CreateHandler();
            })
            .Build();

        connection1.On<string, string, string, string, DateTime>("ReceiveMessage", (id, content, senderId, senderName, sentAt) =>
        {
            connection1MessageReceived = true;
        });

        connection2.On<string, string, string, string, DateTime>("ReceiveMessage", (id, content, senderId, senderName, sentAt) =>
        {
            connection2MessageReceived = true;
        });

        await connection1.StartAsync();
        await connection2.StartAsync();

        // Act
        await connection1.InvokeAsync("SendMessage", "User1", "Hello from connection 1!");

        // Wait for messages to be received
        var timeout = DateTime.UtcNow.AddSeconds(5);
        while ((!connection1MessageReceived || !connection2MessageReceived) && DateTime.UtcNow < timeout)
        {
            await Task.Delay(50);
        }

        // Assert - both connections should receive the message
        Assert.True(connection1MessageReceived);
        Assert.True(connection2MessageReceived);
        
        await connection1.DisposeAsync();
        await connection2.DisposeAsync();
    }

    [Fact]
    public async Task HealthCheck_ReturnsHealthy()
    {
        // Act
        var response = await _client.GetAsync("/health");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Alive_ReturnsOk()
    {
        // Act
        var response = await _client.GetAsync("/alive");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
