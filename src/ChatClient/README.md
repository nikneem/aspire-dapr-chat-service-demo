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

# Start in development mode (uses development.json config)
npm run dev

# Start in production mode (uses production.json config)
npm run prod

# Start with default configuration (respects NODE_ENV)
npm start
```

The server will start on port 3000 by default, or use the PORT environment variable if set.

## API Endpoints

- `/` - Serves the main chat interface
- `/api/config` - Returns service configuration (API URLs from Aspire)
- `/health` - Health check endpoint

## Configuration

The client uses environment-specific configuration files to determine service URLs:

### Development Mode
- Uses `config/development.json` with localhost URLs
- Enables debug mode and additional logging
- Aspire environment variables override configuration file settings

### Production Mode
- Uses `config/production.json` with production URLs
- Optimized for deployment with reduced logging
- Service URLs point to: `members.aspirichat.com`, `messages.aspirichat.com`, `realtime.aspirichat.com`

### Environment Variables

The application supports these environment variables:

- `NODE_ENV` - Environment mode (development/production)
- `PORT` - Server port (default: 3000)
- `CLIENT_TITLE` - Override client title
- `CLIENT_DEBUG` - Enable/disable debug mode

### Aspire Integration

When running with .NET Aspire, the following environment variables will override the configuration:

- `services__membersapi__http__0` - Members API URL
- `services__messagesapi__http__0` - Messages API URL
- `services__realtimeapi__http__0` - Realtime API URL

See [CONFIG.md](CONFIG.md) for detailed configuration information.

## Docker

The included Dockerfile builds a container that runs the Node.js server in development mode. For production deployments, consider using the nginx-based configuration for serving static files.

## Integration with Aspire

This client is added to the Aspire AppHost using the `AddNpmApp` extension method with references to all the backend APIs. The Aspire orchestration ensures proper service discovery and networking between all components.
