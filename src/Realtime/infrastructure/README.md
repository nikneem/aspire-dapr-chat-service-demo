# Members Service Infrastructure

This folder contains the Bicep infrastructure as code (IaC) templates for deploying the Members service to Azure Container Apps.

## Overview

The Members service is deployed as a containerized application in Azure Container Apps with the following features:

- **Separate Resource Group**: The Members service is deployed in its own resource group for better resource isolation and management
- **Container Apps Integration**: Deployed in the shared Container Apps Environment from the main infrastructure
- **Azure App Configuration**: Configured to use Azure App Configuration for centralized configuration management
- **Dapr Integration**: Enabled with Dapr sidecar for pub/sub messaging and state management
- **Health Checks**: Configured with liveness and readiness probes
- **Auto-scaling**: Configured to scale based on HTTP requests and CPU usage
- **Security**: Uses managed identity and secure secrets management

## Files

- `main.bicep` - Main deployment template that creates the resource group and orchestrates the deployment
- `members-app.bicep` - Container App deployment template with full configuration
- `main.dev.bicepparam` - Development environment parameters
- `main.prod.bicepparam` - Production environment parameters
- `README.md` - This documentation file

## Prerequisites

1. **Shared Infrastructure**: The main infrastructure must be deployed first to create:
   - Container Apps Environment
   - Azure App Configuration
   - Application Insights
   - Log Analytics Workspace

2. **Container Registry**: A container registry with the Members API container image

3. **Azure Storage Account**: For Azure Table Storage (used by the Members service)

4. **Azure CLI or PowerShell**: For deployment

## Configuration

### Required Parameters

Before deploying, update the parameter files with actual values:

#### Container Apps Environment
```bicep
param containerAppsEnvironmentId = '/subscriptions/{subscription-id}/resourceGroups/{shared-rg}/providers/Microsoft.App/managedEnvironments/{environment-name}'
```

#### App Configuration
```bicep
param appConfigurationEndpoint = 'https://{app-config-name}.azconfig.io'
```

#### Application Insights
```bicep
param applicationInsightsConnectionString = 'InstrumentationKey={key};IngestionEndpoint=https://westeurope-5.in.applicationinsights.azure.com/;LiveEndpoint=https://westeurope.livediagnostics.monitor.azure.com/'
```

#### Container Registry
```bicep
param containerRegistryServer = '{your-registry}.azurecr.io'
param containerRegistryUsername = '{registry-username}'
param containerRegistryPassword = '{registry-password}'
```

#### Storage Account
```bicep
param tableStorageConnectionString = 'DefaultEndpointsProtocol=https;AccountName={storage-account};AccountKey={account-key};EndpointSuffix=core.windows.net'
```

## Deployment

### Using Azure CLI

#### Development Environment
```bash
# Deploy to development
az deployment sub create \
  --location "West Europe" \
  --template-file main.bicep \
  --parameters main.dev.bicepparam
```

#### Production Environment
```bash
# Deploy to production
az deployment sub create \
  --location "West Europe" \
  --template-file main.bicep \
  --parameters main.prod.bicepparam
```

### Using Azure PowerShell

#### Development Environment
```powershell
# Deploy to development
New-AzSubscriptionDeployment `
  -Location "West Europe" `
  -TemplateFile "main.bicep" `
  -TemplateParameterFile "main.dev.bicepparam"
```

#### Production Environment
```powershell
# Deploy to production
New-AzSubscriptionDeployment `
  -Location "West Europe" `
  -TemplateFile "main.bicep" `
  -TemplateParameterFile "main.prod.bicepparam"
```

## Container App Configuration

The Members service Container App is configured with:

### Environment Variables
- `ASPNETCORE_ENVIRONMENT` - Set based on deployment environment
- `ASPNETCORE_URLS` - HTTP binding on port 8080
- `APPLICATIONINSIGHTS_CONNECTION_STRING` - Application Insights integration
- `ConnectionStrings__AppConfig` - Azure App Configuration endpoint
- `ConnectionStrings__TableStorage` - Azure Table Storage connection
- `Dapr__*` - Dapr configuration settings

### Resource Allocation
- **CPU**: 0.25 cores
- **Memory**: 0.5 GiB
- **Scaling**: 1-10 replicas based on load

### Health Checks
- **Liveness Probe**: `/health` endpoint
- **Readiness Probe**: `/health/ready` endpoint

### Dapr Configuration
- **App ID**: `members-api`
- **App Port**: 8080
- **Protocol**: HTTP
- **Pub/Sub Component**: Configurable via parameters
- **State Store Component**: Configurable via parameters

## Security

- Container registry credentials stored as Container App secrets
- Table Storage connection string stored as secret
- Application Insights connection string stored as secret
- HTTPS-only ingress with TLS termination
- Managed identity integration for Azure services

## Monitoring

The deployment includes:

- Application Insights integration for telemetry
- Log Analytics workspace integration for logs
- Dapr telemetry and logging
- Container App metrics and logs

## Networking

- External ingress enabled for API access
- HTTPS-only traffic
- Integration with Container Apps Environment networking
- Dapr sidecar communication on standard ports

## Outputs

The deployment provides the following outputs:

- `resourceGroupName` - Name of the created resource group
- `containerAppName` - Name of the deployed Container App
- `containerAppUrl` - HTTPS URL of the deployed service
- `containerAppFqdn` - Fully qualified domain name

## Troubleshooting

### Common Issues

1. **Container App fails to start**
   - Check container image availability in registry
   - Verify registry credentials
   - Check environment variable configuration

2. **Health checks failing**
   - Ensure the Members API implements `/health` and `/health/ready` endpoints
   - Verify the application starts correctly on port 8080

3. **Dapr integration issues**
   - Ensure Dapr components are deployed in the Container Apps Environment
   - Verify component names match the parameters
   - Check Dapr logs in the Container App

4. **Azure App Configuration access**
   - Verify the App Configuration endpoint is correct
   - Ensure the Container App has appropriate permissions
   - Check if managed identity is properly configured

### Useful Commands

```bash
# Check deployment status
az deployment sub show --name "{deployment-name}"

# View Container App logs
az containerapp logs show --name "{container-app-name}" --resource-group "{resource-group-name}"

# Check Container App status
az containerapp show --name "{container-app-name}" --resource-group "{resource-group-name}"
```

## Best Practices

1. **Secrets Management**: Use Azure Key Vault for production secrets
2. **Managed Identity**: Configure managed identity for Azure service access
3. **Resource Tagging**: Maintain consistent tagging for cost tracking
4. **Monitoring**: Set up alerts and monitoring dashboards
5. **Backup**: Implement backup strategies for persistent data
6. **Security**: Regular security reviews and updates
7. **Cost Optimization**: Monitor and optimize resource usage
