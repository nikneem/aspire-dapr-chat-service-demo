# Development Environment Configuration

This file contains instructions for running the chat client in development mode.

## Development Mode

To run the chat client in development mode with local service URLs:

```bash
npm run dev
```

This will:
- Set NODE_ENV to 'development'
- Load configuration from `config/development.json`
- Use localhost URLs for all services
- Enable debug mode

## Configuration Files

- `config/development.json` - Development environment settings
- `config/production.json` - Production environment settings (used in Docker builds)

## Environment Variables

The application supports these environment variables:

- `NODE_ENV` - Environment mode (development/production)
- `PORT` - Server port (default: 3000)
- `CLIENT_TITLE` - Override client title
- `CLIENT_DEBUG` - Enable/disable debug mode

## Aspire Integration

When running with .NET Aspire, the following environment variables will override the configuration:

- `services__membersapi__http__0` - Members API URL
- `services__messagesapi__http__0` - Messages API URL  
- `services__realtimeapi__http__0` - Realtime API URL

## Configuration Loading Order

1. Load base configuration from `config/{environment}.json`
2. Apply Aspire environment variable overrides
3. Apply other environment variable overrides
4. Fallback to default localhost URLs if all else fails
