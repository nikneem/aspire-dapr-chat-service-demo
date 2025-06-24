using HexMaster.Chat.Realtime.Api.Hubs;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add controllers for Dapr event handlers
builder.Services.AddControllers();
builder.Services.AddSignalR(options =>
{
    // Configure SignalR options for better CORS support
    options.EnableDetailedErrors = builder.Environment.IsDevelopment();
});

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        // Allow all localhost origins for development
        policy.SetIsOriginAllowed(origin =>
        {
            if (string.IsNullOrEmpty(origin)) return false;

            var uri = new Uri(origin);

            // Allow all localhost and 127.0.0.1 addresses for development
            if (uri.Host == "localhost" || uri.Host == "127.0.0.1")
                return true;

            // In production, you would configure specific allowed origins
            // For now, allowing all for development purposes
            return builder.Environment.IsDevelopment();
        })
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials();
    });
});

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure CORS middleware before other middleware
app.UseCors();

// Map controllers for Dapr subscriptions
app.MapControllers();

// Map SignalR hub with CORS support
app.MapHub<ChatHub>("/chathub");

app.Run();
