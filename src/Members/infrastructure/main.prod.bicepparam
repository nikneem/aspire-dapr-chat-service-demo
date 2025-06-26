using 'main.bicep'

// Production Environment Parameters for Members Service
param location = 'West Europe'
param environment = 'prod'
param appName = 'aspirichat'

// These values should be provided during deployment or from Azure DevOps/GitHub Actions variables
// Container Apps Environment ID from shared infrastructure deployment
param containerAppsEnvironmentId = '/subscriptions/{subscription-id}/resourceGroups/aspirichat-prod-rg/providers/Microsoft.App/managedEnvironments/{container-apps-environment-name}'

// App Configuration endpoint from shared infrastructure deployment
param appConfigurationEndpoint = 'https://{app-config-name}.azconfig.io'

// Application Insights connection string from shared infrastructure deployment
param applicationInsightsConnectionString = 'InstrumentationKey={key};IngestionEndpoint=https://westeurope-5.in.applicationinsights.azure.com/;LiveEndpoint=https://westeurope.livediagnostics.monitor.azure.com/'

// Container configuration
param containerImageTag = 'stable'
param containerRegistryServer = 'docker.io'

// Storage configuration
param tableStorageConnectionString = 'DefaultEndpointsProtocol=https;AccountName={storage-account};AccountKey={account-key};EndpointSuffix=core.windows.net'

// Dapr component names (should match the components deployed in Container Apps Environment)
param daprPubSubComponentName = 'pubsub'
param daprStateStoreComponentName = 'statestore'

param tags = {
  Environment: 'Production'
  Application: 'HexMaster Chat'
  Service: 'Members'
  CreatedBy: 'Bicep'
  Owner: 'Production Team'
  CostCenter: 'Engineering'
  Project: 'AspireChat'
}
