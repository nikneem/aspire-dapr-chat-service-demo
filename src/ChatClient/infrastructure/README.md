# ChatClient Infrastructure

This directory contains the Azure infrastructure templates for the ChatClient service.

## Structure

- `main.bicep` - Main template that creates the resource group and deploys the ChatClient container app
- `resources.bicep` - Defines the Container App and related resources
- `main.dev.bicepparam` - Development environment parameters
- `main.prod.bicepparam` - Production environment parameters

## Resources Created

- **Container App**: Hosts the Node.js ChatClient application
- **Role Assignments**: Grants the Container App access to Azure App Configuration

## Configuration

The ChatClient container app:
- Runs on port 3000
- Uses minimal resources (0.25 CPU, 0.5GB memory)
- Scales from 1 to 5 replicas based on HTTP requests and CPU usage
- Includes health checks on `/health` endpoint
- Has access to Azure App Configuration for service discovery

## Environment Variables

- `NODE_ENV`: Set to 'development' or 'production'
- `PORT`: Set to 3000
- `APPLICATIONINSIGHTS_CONNECTION_STRING`: For telemetry
- `APP_CONFIG_ENDPOINT`: Azure App Configuration endpoint

## Deployment

The infrastructure is deployed via GitHub Actions workflow in `.github/workflows/deploy-client.yml`
