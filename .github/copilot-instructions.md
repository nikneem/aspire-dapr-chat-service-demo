# Chat Application Backend - Copilot Instructions

## Project Overview

This is a distributed chat application backend built with .NET 9 and C#. The application consists of multiple microservices orchestrated with .NET Aspire and uses Dapr for event-driven communication.

## Architecture

- **Members API**: Manages chat members and user registration
- **Messages API**: Handles message sending and storage
- **Realtime API**: Broadcasts messages to clients via SignalR
- **Chat Client**: Node.js-based web client serving the chat interface
- **Aspire AppHost**: Orchestrates all services and dependencies

## Technology Stack

- **.NET 9**: Latest .NET version for all projects
- **ASP.NET Core Web APIs**: RESTful service endpoints
- **Node.js & Express**: Chat client web server
- **Azure Table Storage**: Data persistence for members and messages
- **Dapr**: Event-driven pub/sub messaging
- **Redis**: Dapr state store and pub/sub backend
- **SignalR**: Real-time client communication
- **.NET Aspire**: Service orchestration and observability

## Service Responsibilities

### Members API (`HexMaster.Chat.Members.Api`)

- Register new chat members
- Manage member profiles and status
- Store member data in Azure Table Storage
- Publish member events via Dapr pub/sub
- **Abstractions project** (`HexMaster.Chat.Members.Abstractions`): Contains interfaces, DTOs, events, and request models
- **Implementation project** (`HexMaster.Chat.Members`): Contains concrete implementations, entities, and dependency injection setup

### Messages API (`HexMaster.Chat.Messages.Api`)

- Accept and validate incoming messages
- Store messages in Azure Table Storage
- Publish message events via Dapr pub/sub
- Handle message history retrieval
- **Abstractions project** (`HexMaster.Chat.Messages.Abstractions`): Contains interfaces, DTOs, events, and request models
- **Implementation project** (`HexMaster.Chat.Messages`): Contains concrete implementations, entities, controllers, and dependency injection setup

### Realtime API (`HexMaster.Chat.Realtime.Api`)

- Subscribe to Dapr pub/sub events
- Broadcast messages to connected clients via SignalR
- Manage SignalR connections and groups
- Handle real-time notifications

### Chat Client (`src/ChatClient`)

- Serves the chat application UI via Express.js server
- Provides configuration endpoint exposing Aspire service URLs
- Handles static file serving for the web interface
- Integrates with .NET Aspire orchestration via AddNpmApp

## Project Structure

```
src/
├── Aspire/
│   ├── HexMaster.Chat.Aspire.AppHost/          # Aspire orchestration
│   │   ├── Program.cs                          # Service configuration
│   │   ├── appsettings.json                    # Environment settings
│   │   └── HexMaster.Chat.Aspire.AppHost.csproj
│   └── HexMaster.Chat.Aspire.ServiceDefaults/  # Shared configuration
│       ├── Extensions.cs                       # Common service extensions
│       └── HexMaster.Chat.Aspire.ServiceDefaults.csproj
├── ChatClient/                                  # Node.js web client
│   ├── public/
│   │   └── index.html                          # Chat interface
│   ├── server.js                               # Express.js server
│   ├── package.json                            # Node.js dependencies
│   ├── Dockerfile                              # Container configuration
│   └── README.md                               # Client documentation
├── Members/
│   ├── HexMaster.Chat.Members.Abstractions/    # Member service contracts
│   │   ├── DTOs/                               # Data transfer objects
│   │   ├── Events/                             # Member-specific events
│   │   ├── Interfaces/                         # Service and repository interfaces
│   │   └── Requests/                           # Member-specific request models
│   ├── HexMaster.Chat.Members/                 # Member service implementation
│   │   ├── Entities/
│   │   │   └── MemberEntity.cs                 # Member data model
│   │   ├── Repositories/
│   │   │   └── MemberRepository.cs             # Data access implementation
│   │   ├── Services/
│   │   │   └── MemberService.cs                # Business logic implementation
│   │   ├── BackgroundServices/
│   │   │   └── MemberCleanupService.cs         # Inactive member cleanup
│   │   └── Extensions/
│   │       └── ServiceCollectionExtensions.cs  # Dependency injection setup
│   ├── HexMaster.Chat.Members.Api/             # Member API host
│   │   ├── Program.cs                          # Service configuration
│   │   └── appsettings.json                    # API-specific settings
│   └── HexMaster.Chat.Members.Tests/           # Member service tests
├── Messages/
│   ├── HexMaster.Chat.Messages.Abstractions/   # Message service contracts
│   │   ├── DTOs/                               # Data transfer objects
│   │   ├── Events/                             # Message-specific events
│   │   ├── Interfaces/                         # Service and repository interfaces
│   │   └── Requests/                           # Message-specific request models
│   ├── HexMaster.Chat.Messages/                # Message service implementation
│   │   ├── Entities/
│   │   │   └── MessageEntity.cs                # Message data model
│   │   ├── Repositories/
│   │   │   └── MessageRepository.cs            # Data access implementation
│   │   ├── Services/
│   │   │   ├── MessageService.cs               # Business logic implementation
│   │   │   └── MemberStateService.cs           # Member state management
│   │   ├── BackgroundServices/
│   │   │   └── MessageCleanupService.cs        # Message retention
│   │   ├── Controllers/                        # Dapr event controllers
│   │   └── Extensions/
│   │       └── ServiceCollectionExtensions.cs  # Dependency injection setup
│   ├── HexMaster.Chat.Messages.Api/            # Message API host
│   │   ├── Program.cs                          # Service configuration
│   │   └── appsettings.json                    # API-specific settings
│   └── HexMaster.Chat.Messages.Tests/          # Message service tests
├── Realtime/
│   └── HexMaster.Chat.Realtime.Api/            # SignalR service
│       ├── Controllers/                        # Dapr event controllers
│       ├── Hubs/
│       │   └── ChatHub.cs                      # SignalR hub
│       └── Program.cs                          # Service configuration
├── Shared/
│   └── HexMaster.Chat.Shared/                  # Common models and utilities
│       ├── Constants/                          # Application constants
│       ├── Events/                             # Dapr event models
│       ├── Models/                             # Shared DTOs
│       └── Requests/                           # API request models
├── AspireConstants.cs                          # Service name constants
└── Aspire Chat.sln                            # Solution file
```

## Development Guidelines

### Code Style

- Use C# 13+ features and latest language constructs
- Follow .NET naming conventions (PascalCase for public members, camelCase for parameters)
- Use minimal APIs where appropriate
- Implement proper async/await patterns
- Use record types for DTOs and value objects

### Project Separation

- **Abstractions projects**: Define contracts, interfaces, DTOs, events, and request models
- **Implementation projects**: Contain concrete implementations, entities, repositories, services, and background services
- **API projects**: Host the services and define endpoints, reference both abstractions and implementation projects
- Use extension methods in implementation projects for dependency injection (e.g., `builder.AddChatMembers()`)
- Keep abstractions lightweight with minimal dependencies

### Error Handling

- Implement global exception handling middleware
- Use Problem Details (RFC 7807) for error responses
- Log errors with structured logging using ILogger
- Handle Dapr communication failures gracefully

### Data Access

- Use Azure.Data.Tables NuGet package for Table Storage
- Implement repository pattern for data access
- Use partition keys and row keys effectively for Table Storage
- Handle Azure Table Storage exceptions (throttling, timeouts)

### Dapr Integration

- Use Dapr .NET SDK for pub/sub operations
- Configure Dapr components in the Aspire AppHost
- Use topic-based messaging for loose coupling
- Implement idempotent message handlers

### Aspire Configuration

- Define all services in the AppHost Program.cs
- Configure Dapr sidecars for each API service
- Set up Redis as Dapr state store and pub/sub backend
- Use Aspire service discovery for inter-service communication

### SignalR Implementation

- Use typed hubs for better maintainability
- Implement connection management and authentication
- Handle client disconnections gracefully
- Use groups for targeted message broadcasting

### Chat Client Development

- Use Express.js for serving the web interface
- Implement configuration endpoints for Aspire service discovery
- Use environment variables for service URL configuration
- Follow Node.js best practices for error handling and logging
- Ensure CORS is properly configured for cross-origin requests
- Use npm scripts for development and production builds

## Common Patterns

### Message Flow

1. Client sends message to Messages API
2. Messages API validates and stores message
3. Messages API publishes event via Dapr
4. Realtime API receives event and broadcasts via SignalR

### Member Registration Flow

1. New member registers via Members API
2. Members API stores member data
3. Members API publishes member joined event
4. Realtime API notifies existing clients

### Chat Client Configuration Flow

1. Chat Client starts and requests configuration from `/api/config`
2. Server responds with Aspire-discovered service URLs
3. Client uses these URLs for API communication
4. Automatic service discovery ensures proper connectivity

### Configuration

- Use appsettings.json for environment-specific config
- Leverage Aspire configuration management
- Use Azure Key Vault for secrets in production
- Configure Dapr components declaratively

### Testing

- Write unit tests for business logic
- Use integration tests for API endpoints
- Mock Dapr and Azure Table Storage in tests
- Test SignalR hubs with test clients

### Logging and Monitoring

- Use structured logging with Serilog or built-in ILogger
- Implement correlation IDs for distributed tracing
- Use Aspire dashboard for observability
- Monitor Dapr metrics and health endpoints

## File Organization

- Keep controllers lightweight, delegate to services
- Use separate projects for domain models if needed
- Organize Dapr components in dedicated folders
- Follow clean architecture principles

## Dependencies

- Aspire.Hosting for orchestration
- Aspire.Hosting.NodeJs for Node.js integration
- Dapr.AspNetCore for pub/sub integration
- Azure.Data.Tables for storage
- Microsoft.AspNetCore.SignalR for real-time communication
- Microsoft.Extensions.Hosting for background services
- Express.js for the Node.js web server
- CORS for cross-origin resource sharing

When suggesting code changes or new features, ensure they align with this distributed architecture and maintain consistency across all services.
