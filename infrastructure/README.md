# Infrastructure Deployment

This folder contains the Bicep templates and parameter files for deploying the HexMaster Chat application infrastructure to Azure.

## Files

- `main.bicep` - Main template that creates the resource group and calls the resources module
- `resources.bicep` - Contains all the Azure resources needed for the application
- `main.dev.bicepparam` - Development environment parameters
- `main.staging.bicepparam` - Staging environment parameters
- `main.prod.bicepparam` - Production environment parameters

## Resources Deployed

- **Azure Service Bus (Standard)** - For pub/sub messaging between services
- **Azure Container Apps Environment** - For hosting containerized microservices
- **Log Analytics Workspace** - For centralized logging
- **Application Insights** - For telemetry and monitoring
- **Azure App Configuration (Free tier)** - For centralized configuration management
- **Azure Cache for Redis (Basic C0)** - For state management and caching

## Deployment Commands

### Prerequisites

Make sure you have the Azure CLI installed and are logged in:

```bash
az login
```

### Deploy to Development Environment

```bash
az deployment sub create \
  --location "West Europe" \
  --template-file main.bicep \
  --parameters main.dev.bicepparam
```

### Deploy to Staging Environment

```bash
az deployment sub create \
  --location "West Europe" \
  --template-file main.bicep \
  --parameters main.staging.bicepparam
```

### Deploy to Production Environment

```bash
az deployment sub create \
  --location "West Europe" \
  --template-file main.bicep \
  --parameters main.prod.bicepparam
```

### What-If Deployment (Preview Changes)

You can preview what changes will be made before deploying:

```bash
az deployment sub what-if \
  --location "West Europe" \
  --template-file main.bicep \
  --parameters main.dev.bicepparam
```

## Environment-Specific Configuration

Each environment has its own parameter file with appropriate tags:

- **Development** (`main.dev.bicepparam`): Uses cheapest tiers, minimal retention
- **Staging** (`main.staging.bicepparam`): Production-like configuration for testing
- **Production** (`main.prod.bicepparam`): Enhanced tags for compliance and monitoring

## Security Considerations

- All resources use managed identities where possible
- TLS 1.2 is enforced across all services
- Connection strings and secrets are not exposed in outputs
- Public network access is controlled appropriately

## Cost Optimization

The templates are configured to use cost-effective tiers:

- App Configuration: Free tier
- Redis Cache: Basic C0 (cheapest available)
- Service Bus: Standard (required for topics/subscriptions)
- Log Analytics: Pay-per-GB with 30-day retention

## Post-Deployment Steps

After deployment, you'll need to:

1. Retrieve connection strings from the Azure portal or via Azure CLI
2. Configure your application settings with the deployed resource endpoints
3. Set up Dapr components to use the deployed Redis cache and Service Bus
4. Configure your Container Apps to use the deployed environment

## Outputs

The deployment will output the following values that you'll need for application configuration:

- Resource Group Name
- Service Bus Namespace Name and Endpoint
- Container Apps Environment ID and Name
- Log Analytics Workspace details
- Application Insights connection details
- App Configuration Name and Endpoint
- Redis Cache connection details
