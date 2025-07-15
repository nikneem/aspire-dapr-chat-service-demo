targetScope = 'subscription'

@description('The location for all resources')
param location string = deployment().location

@description('Tags to apply to all resources')
param tags object = {}

@description('Environment name (dev, staging, prod)')
param environment string

@description('Application name prefix')
param appName string

param applicationLandingZone object

@description('Container image tag or version')
param containerImageTag string = 'latest'

@description('Container registry server')
param containerRegistryServer string

var containerApp = toLower('${appName}-${tags.Service}')
var containerAppName = toLower ('${containerApp}-${environment}')
var resourceGroupName = toLower('${containerAppName}-rg')

// Create Resource Group for Messages service
resource resourceGroup 'Microsoft.Resources/resourceGroups@2021-04-01' = {
  name: resourceGroupName
  location: location
  tags: union(tags, {
    Service: tags.Service
    Component: 'API'
  })
}

// Deploy Members Container App
module membersApp 'realtime-app.bicep' = {
  name: 'realtime-app-deployment'
  scope: resourceGroup
  params: {
    location: location
    environment: environment
    containerAppName: containerAppName
    daprId: containerApp
    applicationLandingZone: applicationLandingZone
    containerImageTag: containerImageTag
    containerRegistryServer: containerRegistryServer
    tags: union(tags, {
      Service: tags.Service
      Component: 'API'
    })
  }
}

// Outputs
output resourceGroupName string = resourceGroup.name
output containerAppName string = membersApp.outputs.containerAppName
output containerAppUrl string = membersApp.outputs.containerAppUrl
output containerAppFqdn string = membersApp.outputs.containerAppFqdn
