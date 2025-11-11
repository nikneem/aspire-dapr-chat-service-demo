# Chat Application Backend - Copilot AI Automation Instructions

## Project Overview

This is a distributed chat application backend built with **.NET 10** and **C# 13**. The application consists of multiple microservices orchestrated with .NET Aspire and uses Dapr for event-driven communication. This document establishes clear boundaries and guidelines for AI-assisted development.

## AI Automation Boundaries

### ✅ AI IS ENCOURAGED TO:

- Implement new features following established architectural patterns
- Fix bugs and optimize existing code
- Add comprehensive unit and integration tests
- Improve error handling and logging
- Generate documentation and code comments
- Refactor code for better maintainability
- Add validation and security improvements
- Optimize performance and resource usage
- Use the **microsoft.docs.mcp MCP server** for official Microsoft documentation references

### ⚠️ AI MUST ASK PERMISSION BEFORE:

- Making breaking changes to public APIs or contracts
- Modifying database schemas or storage structures
- Changing Dapr component configurations
- Altering Aspire service orchestration setup
- Modifying infrastructure or deployment configurations
- Adding new external dependencies or NuGet packages
- Changing authentication or authorization mechanisms

### ❌ AI SHOULD NEVER:

- Delete or significantly modify existing data entities
- Remove established security measures
- Make changes that could cause data loss
- Modify production configuration files without explicit instruction
- Change fundamental architectural decisions
- Remove existing tests without replacement

## Architecture

- **Members API**: Manages chat members and user registration
- **Messages API**: Handles message sending and storage
- **Realtime API**: Broadcasts messages to clients via SignalR
- **Chat Client**: Node.js Express server serving the chat interface
- **Aspire AppHost**: Orchestrates all services and dependencies

## Technology Stack

- **.NET 10**: All .NET projects must target .NET 10
- **C# 13**: Use latest C# 13 language features and constructs
- **ASP.NET Core Web APIs**: RESTful service endpoints
- **Node.js >= 20.12 & Express.js**: Chat client web server
- **Azure Table Storage**: Data persistence for members and messages
- **Dapr**: Event-driven pub/sub messaging and state management
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
- **Tests project** (`HexMaster.Chat.Members.Tests`): Comprehensive unit and integration tests

### Messages API (`HexMaster.Chat.Messages.Api`)

- Accept and validate incoming messages
- Store messages in Azure Table Storage
- Publish message events via Dapr pub/sub
- Handle message history retrieval
- **Abstractions project** (`HexMaster.Chat.Messages.Abstractions`): Contains interfaces, DTOs, events, and request models
- **Implementation project** (`HexMaster.Chat.Messages`): Contains concrete implementations, entities, controllers, and dependency injection setup
- **Tests project** (`HexMaster.Chat.Messages.Tests`): Comprehensive unit and integration tests

### Realtime API (`HexMaster.Chat.Realtime.Api`)

- Subscribe to Dapr pub/sub events
- Broadcast messages to connected clients via SignalR
- Manage SignalR connections and groups
- Handle real-time notifications
- **Tests project** (`HexMaster.Chat.Realtime.Tests`): SignalR hub and real-time communication tests

### Chat Client (`src/ChatClient`)

- **Node.js >= 20.12** Express.js server serving the chat interface
- Provides configuration endpoint exposing Aspire service URLs via `/api/config`
- Handles static file serving for the web interface
- Integrates with .NET Aspire orchestration via AddNpmApp
- Uses configuration loader for environment-specific settings
- Health check endpoint at `/health`

### Shared Components (`HexMaster.Chat.Shared`)

- Common models, constants, and utilities shared across all services
- Dapr event models and shared DTOs
- Application-wide constants and configuration models

### Aspire Configuration

- **AppHost** (`HexMaster.Chat.Aspire.AppHost`): Central orchestration and service configuration
- **ServiceDefaults** (`HexMaster.Chat.Aspire.ServiceDefaults`): Common service extensions and configurations

## Project Structure

```
src/
├── .gitignore                                   # Git ignore rules
├── .vs/                                         # Visual Studio files
├── Aspire Chat.sln                              # Solution file
├── AspireConstants.cs                           # Service name constants
├── Aspire/
│   └── HexMaster.Chat.Aspire/
│       ├── HexMaster.Chat.Aspire.AppHost/       # Aspire orchestration
│       │   ├── Program.cs                       # Service configuration
│       │   ├── appsettings.json                 # Environment settings
│       │   └── HexMaster.Chat.Aspire.AppHost.csproj
│       └── HexMaster.Chat.Aspire.ServiceDefaults/ # Shared configuration
│           ├── Extensions.cs                    # Common service extensions
│           └── HexMaster.Chat.Aspire.ServiceDefaults.csproj
├── ChatClient/                                  # Node.js Express web server
│   ├── build-prod.bat                           # Windows production build
│   ├── build-prod.js                            # Production build script
│   ├── build-prod.sh                            # Unix production build
│   ├── config-loader.js                         # Configuration management
│   ├── CONFIG.md                                # Configuration documentation
│   ├── Dockerfile                               # Container configuration
│   ├── nginx.conf.template                      # Nginx configuration template
│   ├── package.json                             # Node.js dependencies (>=20.12)
│   ├── README.md                                # Client documentation
│   ├── server.js                                # Express.js server entry point
│   ├── test-config.js                           # Configuration testing
│   ├── config/                                  # Environment configurations
│   ├── infrastructure/                          # Deployment configurations
│   └── public/                                  # Static web files
├── Members/
│   ├── HexMaster.Chat.Members.Abstractions/     # Member service contracts
│   │   ├── DTOs/                                # Data transfer objects
│   │   ├── Events/                              # Member-specific events
│   │   ├── Interfaces/                          # Service and repository interfaces
│   │   └── Requests/                            # Member-specific request models
│   ├── HexMaster.Chat.Members/                  # Member service implementation
│   │   ├── Entities/
│   │   │   └── MemberEntity.cs                  # Member data model
│   │   ├── Repositories/
│   │   │   └── MemberRepository.cs              # Data access implementation
│   │   ├── Services/
│   │   │   └── MemberService.cs                 # Business logic implementation
│   │   ├── BackgroundServices/
│   │   │   └── MemberCleanupService.cs          # Inactive member cleanup
│   │   └── Extensions/
│   │       └── ServiceCollectionExtensions.cs   # Dependency injection setup
│   ├── HexMaster.Chat.Members.Api/              # Member API host
│   │   ├── Program.cs                           # Service configuration
│   │   └── appsettings.json                     # API-specific settings
│   ├── HexMaster.Chat.Members.Tests/            # Member service tests
│   └── infrastructure/                          # Member service infrastructure
├── Messages/
│   ├── HexMaster.Chat.Messages.Abstractions/    # Message service contracts
│   │   ├── DTOs/                                # Data transfer objects
│   │   ├── Events/                              # Message-specific events
│   │   ├── Interfaces/                          # Service and repository interfaces
│   │   └── Requests/                            # Message-specific request models
│   ├── HexMaster.Chat.Messages/                 # Message service implementation
│   │   ├── Entities/
│   │   │   └── MessageEntity.cs                 # Message data model
│   │   ├── Repositories/
│   │   │   └── MessageRepository.cs             # Data access implementation
│   │   ├── Services/
│   │   │   ├── MessageService.cs                # Business logic implementation
│   │   │   └── MemberStateService.cs            # Member state management
│   │   ├── BackgroundServices/
│   │   │   └── MessageCleanupService.cs         # Message retention
│   │   ├── Controllers/                         # Dapr event controllers
│   │   └── Extensions/
│   │       └── ServiceCollectionExtensions.cs   # Dependency injection setup
│   ├── HexMaster.Chat.Messages.Api/             # Message API host
│   │   ├── Program.cs                           # Service configuration
│   │   └── appsettings.json                     # API-specific settings
│   ├── HexMaster.Chat.Messages.Tests/           # Message service tests
│   └── infrastructure/                          # Message service infrastructure
├── Realtime/
│   ├── HexMaster.Chat.Realtime.Api/             # SignalR service
│   │   ├── Controllers/                         # Dapr event controllers
│   │   ├── Hubs/
│   │   │   └── ChatHub.cs                       # SignalR hub
│   │   └── Program.cs                           # Service configuration
│   ├── HexMaster.Chat.Realtime.Tests/           # Realtime service tests
│   └── infrastructure/                          # Realtime service infrastructure
├── Shared/
│   └── HexMaster.Chat.Shared/                   # Common models and utilities
│       ├── Constants/                           # Application constants
│       ├── Events/                              # Dapr event models
│       ├── Models/                              # Shared DTOs
│       └── Requests/                            # API request models
└── TestResults/                                 # Test execution results
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
