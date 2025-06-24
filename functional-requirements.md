# Functional Requirements - Aspire Dapr Chat Service Demo

## Project Overview

This document outlines the functional requirements for a distributed chat application backend built with .NET 9, ASP.NET Core, .NET Aspire, and Dapr. The system enables real-time messaging between multiple users through a microservices architecture with event-driven communication.

## System Architecture

The application consists of three main microservices:

- **Members API**: Manages user registration and member lifecycle
- **Messages API**: Handles message processing and storage
- **Realtime API**: Provides real-time message broadcasting via SignalR

All services are orchestrated using .NET Aspire and communicate through Dapr pub/sub messaging patterns.

## Infrastructure Requirements

### REQ-INF-001: Aspire Application Host Setup

**Description**: The system must be configured using .NET Aspire for service orchestration and observability.
**Details**:

- Configure all microservices in the Aspire AppHost
- Set up service discovery between components
- Enable distributed tracing and monitoring
- Configure health checks for all services

### REQ-INF-002: Azure Storage Account Emulator

**Description**: The system must use Azure Storage Account emulator for local development and testing.
**Details**:

- Configure Azurite or Azure Storage Emulator for local development
- Set up Table Storage for data persistence
- Ensure proper connection string configuration
- Support both development and production storage configurations

### REQ-INF-003: Dapr Components Configuration

**Description**: Dapr components must be properly configured for pub/sub messaging and state management.
**Details**:

- Configure Redis as the Dapr state store backend
- Set up Redis as the Dapr pub/sub component
- Define topic-based messaging patterns
- Configure Dapr sidecars for each microservice
- Ensure reliable message delivery and ordering

### REQ-INF-004: Service Communication

**Description**: All microservices must communicate through Dapr pub/sub patterns.
**Details**:

- Implement asynchronous event-driven communication
- Use topic-based message routing
- Handle message serialization/deserialization
- Implement retry policies and dead letter queues

## Member Management Requirements

### REQ-MEM-001: Member Registration

**Description**: Users must be able to register and join the chat system.
**Details**:

- Accept member registration requests via REST API
- Validate member information (name, email, display preferences)
- Generate unique member identifiers
- Store member data in Azure Table Storage
- Publish "member-joined" events via Dapr pub/sub
- Return registration confirmation to client

### REQ-MEM-002: Member Status Tracking

**Description**: The system must track member activity and online status.
**Details**:

- Record member last activity timestamp
- Track member connection status
- Update activity on message sending or API interactions
- Provide member presence information to other services

### REQ-MEM-003: Member Data Persistence

**Description**: Member information must be stored reliably in Azure Table Storage.
**Details**:

- Use efficient partition and row key strategies
- Store member profile information
- Maintain activity timestamps
- Support member data retrieval by ID and bulk operations

## Message Processing Requirements

### REQ-MSG-001: Message Acceptance

**Description**: The system must accept and validate incoming chat messages.
**Details**:

- Accept messages via REST API endpoints
- Validate message content (length, format, profanity filtering)
- Verify sender authentication and authorization
- Assign unique message identifiers and timestamps
- Support different message types (text, emoji, system messages)

### REQ-MSG-002: Message Storage

**Description**: All messages must be persisted in Azure Table Storage.
**Details**:

- Store messages with efficient partitioning strategy
- Include sender information, timestamp, and content
- Support message threading and conversation grouping
- Maintain message ordering and delivery status

### REQ-MSG-003: Message Publishing

**Description**: Valid messages must be published for real-time delivery.
**Details**:

- Publish "message-sent" events via Dapr pub/sub
- Include message metadata and routing information
- Ensure reliable event delivery
- Handle publishing failures gracefully

### REQ-MSG-004: Message History

**Description**: Users must be able to retrieve chat history.
**Details**:

- Provide paginated message retrieval
- Support filtering by date range and sender
- Return messages in chronological order
- Include sender information and timestamps

## Real-time Communication Requirements

### REQ-RT-001: SignalR Hub Configuration

**Description**: The system must provide real-time message broadcasting via SignalR.
**Details**:

- Configure SignalR hubs for client connections
- Support WebSocket and fallback protocols
- Handle client connection and disconnection events
- Implement connection grouping and targeting

### REQ-RT-002: Event Subscription

**Description**: The Realtime API must subscribe to Dapr pub/sub events.
**Details**:

- Subscribe to "message-sent" events from Messages API
- Subscribe to "member-joined" and "member-left" events from Members API
- Process events asynchronously and reliably
- Handle event processing failures with retry logic

### REQ-RT-003: Message Broadcasting

**Description**: Received events must be broadcast to connected clients.
**Details**:

- Broadcast messages to all connected clients
- Support targeted broadcasting to specific groups
- Include sender information and message metadata
- Handle client disconnections during broadcasting

### REQ-RT-004: Connection Management

**Description**: The system must manage SignalR client connections effectively.
**Details**:

- Track active client connections
- Associate connections with member identities
- Handle connection authentication and authorization
- Implement graceful disconnection handling

## Data Cleanup Requirements

### REQ-CLN-001: Inactive Member Cleanup

**Description**: Members must be automatically removed from storage after one hour of inactivity.
**Details**:

- Implement background service to monitor member activity
- Check last activity timestamp against current time
- Remove members inactive for more than 60 minutes
- Publish "member-left" events for cleaned up members
- Clean up associated data and connections
- Run cleanup process every 15 minutes

### REQ-CLN-002: Message Retention Policy

**Description**: Messages must be automatically removed 24 hours after they were sent.
**Details**:

- Implement background service for message cleanup
- Delete messages older than 24 hours based on timestamp
- Clean up in batches to avoid performance impact
- Log cleanup operations for auditing
- Run cleanup process every hour
- Preserve system messages for longer retention if needed

### REQ-CLN-003: Storage Optimization

**Description**: The cleanup processes must optimize storage usage and performance.
**Details**:

- Use efficient batch operations for bulk deletions
- Minimize impact on active system operations
- Monitor storage usage and cleanup effectiveness
- Implement cleanup failure handling and retry logic

## Requirements Summary Table

| Requirement ID | Category           | Priority | Description                    | Dependencies             |
| -------------- | ------------------ | -------- | ------------------------------ | ------------------------ |
| REQ-INF-001    | Infrastructure     | High     | Aspire Application Host Setup  | -                        |
| REQ-INF-002    | Infrastructure     | High     | Azure Storage Account Emulator | REQ-INF-001              |
| REQ-INF-003    | Infrastructure     | High     | Dapr Components Configuration  | REQ-INF-001              |
| REQ-INF-004    | Infrastructure     | High     | Service Communication          | REQ-INF-003              |
| REQ-MEM-001    | Member Management  | High     | Member Registration            | REQ-INF-002, REQ-INF-003 |
| REQ-MEM-002    | Member Management  | Medium   | Member Status Tracking         | REQ-MEM-001              |
| REQ-MEM-003    | Member Management  | High     | Member Data Persistence        | REQ-INF-002              |
| REQ-MSG-001    | Message Processing | High     | Message Acceptance             | REQ-MEM-001              |
| REQ-MSG-002    | Message Processing | High     | Message Storage                | REQ-INF-002              |
| REQ-MSG-003    | Message Processing | High     | Message Publishing             | REQ-INF-003, REQ-MSG-002 |
| REQ-MSG-004    | Message Processing | Medium   | Message History                | REQ-MSG-002              |
| REQ-RT-001     | Real-time          | High     | SignalR Hub Configuration      | REQ-INF-001              |
| REQ-RT-002     | Real-time          | High     | Event Subscription             | REQ-INF-003, REQ-RT-001  |
| REQ-RT-003     | Real-time          | High     | Message Broadcasting           | REQ-RT-001, REQ-RT-002   |
| REQ-RT-004     | Real-time          | Medium   | Connection Management          | REQ-RT-001               |
| REQ-CLN-001    | Data Cleanup       | Medium   | Inactive Member Cleanup        | REQ-MEM-002, REQ-MEM-003 |
| REQ-CLN-002    | Data Cleanup       | Medium   | Message Retention Policy       | REQ-MSG-002              |
| REQ-CLN-003    | Data Cleanup       | Low      | Storage Optimization           | REQ-CLN-001, REQ-CLN-002 |

## Detailed Requirement Descriptions

### Infrastructure Foundation

The system begins with establishing a robust infrastructure foundation using .NET Aspire for service orchestration. This provides centralized configuration, health monitoring, and service discovery capabilities. The Azure Storage Account emulator serves as the primary data store during development, ensuring compatibility with production Azure Table Storage while enabling local development workflows.

Dapr components form the backbone of inter-service communication, with Redis serving dual roles as both the state store and pub/sub message broker. This configuration ensures reliable, scalable messaging between the three microservices while maintaining loose coupling and resilience.

### Member Lifecycle Management

The member management system handles the complete lifecycle of chat participants. When a user joins the chat, the Members API validates their information, assigns a unique identifier, and stores their profile in Azure Table Storage. The system continuously tracks member activity, updating timestamps whenever members interact with the system.

Member presence is crucial for the real-time experience, so the system maintains current activity status and publishes events when members join or become inactive. This information flows through the Dapr pub/sub system to notify other services and connected clients.

### Message Flow and Processing

The message processing pipeline starts when users send messages through the Messages API. Each message undergoes validation for content, length, and sender authorization before being stored in Azure Table Storage with appropriate metadata including timestamps, sender information, and unique identifiers.

Once stored, the Messages API publishes events through Dapr pub/sub, triggering the real-time broadcasting process. The system maintains message history with efficient retrieval capabilities, supporting pagination and filtering to handle large conversation volumes.

### Real-time Communication

The Realtime API serves as the bridge between the event-driven backend and connected clients. Through SignalR hubs, it maintains persistent connections with chat participants and listens for events from the other microservices via Dapr subscriptions.

When messages or member events are received, the Realtime API immediately broadcasts them to appropriate clients, ensuring minimal latency in message delivery. Connection management handles the complexities of WebSocket connections, including authentication, grouping, and graceful disconnection handling.

### Automated Data Management

The data cleanup system runs as background services within the respective APIs, ensuring optimal storage usage and maintaining chat performance. The member cleanup process runs every 15 minutes, identifying members who haven't been active for over an hour and removing their data while publishing appropriate departure events.

Message cleanup operates on a 24-hour retention policy, running hourly to remove expired messages in batches. This approach balances storage efficiency with system performance, ensuring that cleanup operations don't impact active chat functionality.

Both cleanup processes include comprehensive error handling, retry logic, and audit logging to maintain system reliability and provide operational insights into data management activities.

## Success Criteria

The system successfully meets its functional requirements when:

- All services start and communicate reliably through Aspire orchestration
- Members can register, send messages, and receive real-time updates
- Data cleanup processes maintain optimal storage usage automatically
- The system handles concurrent users and high message volumes
- All components demonstrate resilience to failures and recovery capabilities

This requirements specification serves as the foundation for implementation, testing, and validation of the distributed chat application system.
