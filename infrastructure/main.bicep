targetScope = 'subscription'

@description('The location for all resources')
param location string = deployment().location

@description('Tags to apply to all resources')
param tags object = {}

@description('Environment name (dev, staging, prod)')
param environment string

@description('Application name prefix')
param appName string

var defaultResourceName = '${appName}-${environment}'
var resourceGroupName = '${defaultResourceName}-rg'

// Create Resource Group
resource resourceGroup 'Microsoft.Resources/resourceGroups@2021-04-01' = {
  name: resourceGroupName
  location: location // Default location, can be overridden in parameters
  tags: tags
}

// Deploy all resources
module resources 'resources.bicep' = {
  name: 'resources-deployment'
  scope: resourceGroup
  params: {
    location: location
    environment: environment
    appName: appName
    tags: tags
  }
}

// Outputs
output resourceGroupName string = resourceGroup.name
output serviceBusNamespaceName string = resources.outputs.serviceBusNamespaceName
output serviceBusNamespaceEndpoint string = resources.outputs.serviceBusNamespaceEndpoint
output containerAppsEnvironmentId string = resources.outputs.containerAppsEnvironmentId
output containerAppsEnvironmentName string = resources.outputs.containerAppsEnvironmentName
output logAnalyticsWorkspaceId string = resources.outputs.logAnalyticsWorkspaceId
output logAnalyticsWorkspaceName string = resources.outputs.logAnalyticsWorkspaceName
output applicationInsightsConnectionString string = resources.outputs.applicationInsightsConnectionString
output applicationInsightsInstrumentationKey string = resources.outputs.applicationInsightsInstrumentationKey
output appConfigurationName string = resources.outputs.appConfigurationName
output appConfigurationEndpoint string = resources.outputs.appConfigurationEndpoint
output redisCacheName string = resources.outputs.redisCacheName
output redisCacheHostName string = resources.outputs.redisCacheHostName
output redisCachePort string = resources.outputs.redisCachePort
output redisCacheSslPort string = resources.outputs.redisCacheSslPort


