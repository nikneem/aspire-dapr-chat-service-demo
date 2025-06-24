using HexMaster.Chat.Realtime.Api.Hubs;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add controllers for Dapr event handlers
builder.Services.AddControllers();
builder.Services.AddSignalR();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:3000", "https://localhost:3001") // Add your client URLs
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

var app = builder.Build();

app.MapDefaultEndpoints();
app.UseCors();

// Map controllers for Dapr subscriptions
app.MapControllers();

// Map SignalR hub
app.MapHub<ChatHub>("/chathub");

app.Run();
