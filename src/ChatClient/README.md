# Chat Client

This is a Node.js-based chat client that serves the HTML interface for the distributed chat application. It's orchestrated by .NET Aspire alongside the other microservices.

## Features

- Serves the chat application UI via Express.js server
- Provides configuration endpoint that exposes Aspire service URLs
- Fully integrated with .NET Aspire orchestration
- Docker support for deployment

## Development

The chat client is automatically started when you run the Aspire AppHost. The Node.js dependencies are automatically installed during the build process.

### Manual Setup

If you need to run the client independently:

```bash
# Install dependencies
npm install

# Start the development server
npm start
```

The server will start on port 3000 by default, or use the PORT environment variable if set.

## API Endpoints

- `/` - Serves the main chat interface
- `/api/config` - Returns service configuration (API URLs from Aspire)
- `/health` - Health check endpoint

## Configuration

The client automatically receives service URLs from Aspire through environment variables:

- `services__hexmaster_chat_members_api__http__0` - Members API URL
- `services__hexmaster_chat_messages_api__http__0` - Messages API URL
- `services__hexmaster_chat_realtime_api__http__0` - Realtime API URL

These are exposed to the frontend via the `/api/config` endpoint.

## Docker

The included Dockerfile builds a container that runs the Node.js server in development mode. For production deployments, consider using the nginx-based configuration for serving static files.

## Integration with Aspire

This client is added to the Aspire AppHost using the `AddNpmApp` extension method with references to all the backend APIs. The Aspire orchestration ensures proper service discovery and networking between all components.
