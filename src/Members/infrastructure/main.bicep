targetScope = 'subscription'

@description('The location for all resources')
param location string = deployment().location

@description('Tags to apply to all resources')
param tags object = {}

@description('Environment name (dev, staging, prod)')
param environment string

@description('Application name prefix')
param appName string

@description('Container Apps Environment resource ID from shared infrastructure')
param containerAppsEnvironmentId string

@description('App Configuration endpoint from shared infrastructure')
param appConfigurationEndpoint string

@description('Application Insights connection string from shared infrastructure')
param applicationInsightsConnectionString string

@description('Container image tag or version')
param containerImageTag string = 'latest'

@description('Container registry server')
param containerRegistryServer string

@description('Azure Table Storage connection string')
@secure()
param tableStorageConnectionString string

@description('Dapr pub/sub component name')
param daprPubSubComponentName string = 'pubsub'

@description('Dapr state store component name')
param daprStateStoreComponentName string = 'statestore'

var resourceGroupName = '${appName}-members-${environment}-rg'
var containerAppName = '${appName}-members-${environment}'

// Create Resource Group for Members service
resource resourceGroup 'Microsoft.Resources/resourceGroups@2021-04-01' = {
  name: resourceGroupName
  location: location
  tags: union(tags, {
    Service: 'Members'
    Component: 'API'
  })
}

// Deploy Members Container App
module membersApp 'members-app.bicep' = {
  name: 'members-app-deployment'
  scope: resourceGroup
  params: {
    location: location
    environment: environment
    appName: appName
    containerAppName: containerAppName
    containerAppsEnvironmentId: containerAppsEnvironmentId
    appConfigurationEndpoint: appConfigurationEndpoint
    applicationInsightsConnectionString: applicationInsightsConnectionString
    containerImageTag: containerImageTag
    containerRegistryServer: containerRegistryServer
    tableStorageConnectionString: tableStorageConnectionString
    daprPubSubComponentName: daprPubSubComponentName
    daprStateStoreComponentName: daprStateStoreComponentName
    tags: union(tags, {
      Service: 'Members'
      Component: 'API'
    })
  }
}

// Outputs
output resourceGroupName string = resourceGroup.name
output containerAppName string = membersApp.outputs.containerAppName
output containerAppUrl string = membersApp.outputs.containerAppUrl
output containerAppFqdn string = membersApp.outputs.containerAppFqdn
