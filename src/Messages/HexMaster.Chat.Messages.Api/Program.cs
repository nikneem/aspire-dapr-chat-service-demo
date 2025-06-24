using HexMaster.Chat.Messages.Api.BackgroundServices;
using HexMaster.Chat.Messages.Api.Repositories;
using HexMaster.Chat.Messages.Api.Services;
using HexMaster.Chat.Shared.Requests;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add Azure Table Storage
builder.AddAzureTableClient("tables");

// Add controllers for Dapr event handling
builder.Services.AddControllers().AddDapr();

// Add services
builder.Services.AddScoped<IMessageRepository, MessageRepository>();
builder.Services.AddScoped<IMessageService, MessageService>();
builder.Services.AddScoped<IMemberStateService, MemberStateService>();

// Add background services
builder.Services.AddHostedService<MessageCleanupService>();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

app.MapDefaultEndpoints();
app.UseCors();

// Map controllers for Dapr event handling
app.MapControllers();

// API endpoints
app.MapPost("/messages", async (SendMessageRequest request, IMessageService messageService) =>
{
    if (string.IsNullOrWhiteSpace(request.Content) || string.IsNullOrWhiteSpace(request.SenderId))
    {
        return Results.BadRequest("Content and SenderId are required");
    }

    try
    {
        var message = await messageService.SendMessageAsync(request);
        return Results.Created($"/messages/{message.Id}", message);
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(ex.Message);
    }
});

app.MapGet("/messages/recent", async (IMessageService messageService, int count = 50) =>
{
    var messages = await messageService.GetRecentMessagesAsync(count);
    return Results.Ok(messages);
});

app.MapGet("/messages/history", async (
    IMessageService messageService,
    DateTime? from = null,
    DateTime? to = null) =>
{
    var fromDate = from ?? DateTime.UtcNow.AddDays(-1);
    var toDate = to ?? DateTime.UtcNow;

    var messages = await messageService.GetMessageHistoryAsync(fromDate, toDate);
    return Results.Ok(messages);
});

app.Run();
