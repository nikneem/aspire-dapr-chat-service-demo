# Chat Application Backend - Copilot Instructions

## Project Overview

This is a distributed chat application backend built with .NET 9 and C#. The application consists of multiple microservices orchestrated with .NET Aspire and uses Dapr for event-driven communication.

## Architecture

- **Members API**: Manages chat members and user registration
- **Messages API**: Handles message sending and storage
- **Realtime API**: Broadcasts messages to clients via SignalR
- **Aspire AppHost**: Orchestrates all services and dependencies

## Technology Stack

- **.NET 9**: Latest .NET version for all projects
- **ASP.NET Core Web APIs**: RESTful service endpoints
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

### Messages API (`HexMaster.Chat.Messages.Api`)

- Accept and validate incoming messages
- Store messages in Azure Table Storage
- Publish message events via Dapr pub/sub
- Handle message history retrieval

### Realtime API (`HexMaster.Chat.Realtime.Api`)

- Subscribe to Dapr pub/sub events
- Broadcast messages to connected clients via SignalR
- Manage SignalR connections and groups
- Handle real-time notifications

## Development Guidelines

### Code Style

- Use C# 13+ features and latest language constructs
- Follow .NET naming conventions (PascalCase for public members, camelCase for parameters)
- Use minimal APIs where appropriate
- Implement proper async/await patterns
- Use record types for DTOs and value objects

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
- Dapr.AspNetCore for pub/sub integration
- Azure.Data.Tables for storage
- Microsoft.AspNetCore.SignalR for real-time communication
- Microsoft.Extensions.Hosting for background services

When suggesting code changes or new features, ensure they align with this distributed architecture and maintain consistency across all services.
