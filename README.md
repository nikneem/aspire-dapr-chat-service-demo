# Aspire Dapr Chat Service Demo

A distributed chat application backend built with .NET 9, ASP.NET Core, .NET Aspire, and Dapr. This demonstration showcases event-driven microservices architecture with real-time messaging capabilities.

## Architecture Overview

The application consists of three main microservices:

- **Members API** (`localhost:5196`): Manages user registration and member lifecycle
- **Messages API** (`localhost:5113`): Handles message processing and storage
- **Realtime API** (`localhost:5197`): Provides real-time message broadcasting via SignalR

All services are orchestrated using .NET Aspire and communicate through Dapr pub/sub messaging patterns.

## Technology Stack

- **.NET 9**: Latest .NET version for all projects
- **ASP.NET Core Web APIs**: RESTful service endpoints
- **Azure Table Storage**: Data persistence (using Azurite emulator for local development)
- **Dapr**: Event-driven pub/sub messaging
- **Redis**: Dapr state store and pub/sub backend
- **SignalR**: Real-time client communication
- **.NET Aspire**: Service orchestration and observability

## Prerequisites

Before running the application, ensure you have the following installed:

1. **.NET 9 SDK**: Download from [https://dotnet.microsoft.com/download/dotnet/9.0](https://dotnet.microsoft.com/download/dotnet/9.0)
2. **Docker Desktop**: Required for Redis and Azurite containers
3. **Dapr CLI**: Install from [https://docs.dapr.io/getting-started/install-dapr-cli/](https://docs.dapr.io/getting-started/install-dapr-cli/)
4. **Visual Studio 2022** or **Visual Studio Code** (recommended)

### Installing Dapr

```powershell
# Install Dapr CLI
powershell -Command "iwr -useb https://raw.githubusercontent.com/dapr/cli/master/install/install.ps1 | iex"

# Initialize Dapr (this will install Docker containers for Redis, Zipkin, and Placement service)
dapr init
```

## Getting Started

### 1. Clone and Navigate to Project

```powershell
git clone <repository-url>
cd aspire-dapr-chat-service-demo
```

### 2. Start the Application

#### Option A: Using Visual Studio 2022

1. Open `src/Aspire Chat.sln` in Visual Studio 2022
2. Set `HexMaster.Chat.Aspire.AppHost` as the startup project
3. Press F5 or click "Run"

#### Option B: Using Command Line

```powershell
cd src
dotnet run --project Aspire/HexMaster.Chat.Aspire/HexMaster.Chat.Aspire.AppHost
```

### 3. Access the Application

Once running, you'll have access to:

- **Aspire Dashboard**: `https://localhost:15888` (orchestration and monitoring)
- **Members API**: `http://localhost:5196`
- **Messages API**: `http://localhost:5113`
- **Realtime API**: `http://localhost:5197`
- **Test Client**: Open `test-client.html` in your browser

## API Endpoints

### Members API (`localhost:5196`)

```http
POST /members
Content-Type: application/json
{
  "name": "John Doe",
  "email": "john@example.com"
}

GET /members/{id}

PUT /members/{id}/activity
```

### Messages API (`localhost:5113`)

```http
POST /messages
Content-Type: application/json
{
  "content": "Hello, world!",
  "senderId": "member-id-here"
}

GET /messages/recent?count=50

GET /messages/history?from=2024-01-01&to=2024-01-02
```

### Realtime API (`localhost:5197`)

- **SignalR Hub**: `/chathub`
- **Event Endpoints**: Handled automatically by Dapr subscriptions

## Using the Test Client

1. Open `test-client.html` in your web browser
2. Enter your name and email, then click "Register & Connect"
3. Start typing messages in the message box
4. Open multiple browser tabs to simulate multiple users
5. Watch real-time message delivery across all connected clients

## Event-Driven Architecture

The application uses Dapr pub/sub for loose coupling between services:

### Event Flow

1. **Member Registration**:

   - Members API stores member data
   - Publishes `member-joined` event
   - Realtime API broadcasts to connected clients

2. **Message Sending**:

   - Messages API validates and stores message
   - Publishes `message-sent` event
   - Realtime API broadcasts to all chat participants

3. **Member Cleanup**:

   - Background service removes inactive members (>1 hour)
   - Publishes `member-left` event
   - Realtime API notifies remaining participants

4. **Message Cleanup**:
   - Background service removes old messages (>24 hours)
   - Runs automatically every hour

## Monitoring and Observability

### Aspire Dashboard

Access the Aspire dashboard at `https://localhost:15888` to monitor:

- Service health and status
- Distributed tracing
- Logs aggregation
- Resource utilization
- Service dependencies

### Dapr Dashboard (Optional)

```powershell
dapr dashboard
```

Access at `http://localhost:8080` for Dapr-specific monitoring.

## Data Storage

### Azure Table Storage (Azurite)

The application uses Azure Table Storage for persistence:

- **Members Table**: User profiles and activity tracking
- **Messages Table**: Chat message history

Data is automatically cleaned up based on retention policies:

- Members: Removed after 1 hour of inactivity
- Messages: Removed after 24 hours

### Redis

Used by Dapr for:

- State management
- Pub/sub messaging
- Service-to-service communication

## Development and Debugging

### Running Individual Services

You can run services individually for debugging:

```powershell
# Members API
cd src/Members/HexMaster.Chat.Members.Api
dapr run --app-id members-api --app-port 5196 --dapr-http-port 3501 --components-path ../../../dapr -- dotnet run

# Messages API
cd src/Messages/HexMaster.Chat.Messages.Api
dapr run --app-id messages-api --app-port 5113 --dapr-http-port 3502 --components-path ../../../dapr -- dotnet run

# Realtime API
cd src/Realtime/HexMaster.Chat.Realtime.Api
dapr run --app-id realtime-api --app-port 5197 --dapr-http-port 3503 --components-path ../../../dapr -- dotnet run
```

### Logs and Troubleshooting

- **Aspire Dashboard**: Check the logs tab for each service
- **Console Output**: Each service outputs structured logs to console
- **Dapr Logs**: Use `dapr logs --app-id <service-name>` for Dapr-specific logs

## Production Considerations

This is a demonstration application. For production deployment, consider:

### Security

- Implement proper authentication and authorization
- Use HTTPS for all endpoints
- Secure SignalR connections with authentication
- Validate and sanitize all user inputs

### Scalability

- Use Azure Service Bus instead of Redis for pub/sub in production
- Implement proper database scaling strategies
- Consider Azure SignalR Service for SignalR scaling
- Add rate limiting and throttling

### Reliability

- Implement circuit breakers and retry policies
- Add comprehensive error handling and recovery
- Use Azure Key Vault for secrets management
- Implement health checks and monitoring

### Infrastructure

- Deploy to Azure Container Apps or Azure Kubernetes Service
- Use Azure Table Storage or Azure Cosmos DB for production data
- Implement proper CI/CD pipelines
- Add automated testing suites

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests if applicable
5. Submit a pull request

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Support

For questions or issues:

1. Check the Aspire dashboard for service health
2. Review the console logs for error messages
3. Ensure all prerequisites are properly installed
4. Verify Docker containers are running (`docker ps`)
5. Check Dapr components configuration in the `/dapr` folder

## Architecture Diagram

```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   Members API   │    │  Messages API   │    │  Realtime API   │
│  (Port 5196)    │    │  (Port 5113)    │    │  (Port 5197)    │
└─────────┬───────┘    └─────────┬───────┘    └─────────┬───────┘
          │                      │                      │
          │                      │                      │
          └──────────┬───────────┴──────────┬───────────┘
                     │                      │
                     ▼                      ▼
              ┌─────────────┐        ┌─────────────┐
              │    Dapr     │        │   SignalR   │
              │  Pub/Sub    │        │    Hubs     │
              └─────────────┘        └─────────────┘
                     │
                     ▼
              ┌─────────────┐
              │    Redis    │
              │ (Container) │
              └─────────────┘

              ┌─────────────┐
              │   Azurite   │
              │ (Container) │
              └─────────────┘
```

This architecture ensures loose coupling, scalability, and real-time communication capabilities while maintaining development simplicity and production readiness.
This is an ASP.NET Web API written in C# that allows its visitors to chat
