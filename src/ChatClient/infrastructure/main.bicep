targetScope = 'subscription'

@description('The location for all resources')
param location string = deployment().location

@description('Tags to apply to all resources')
param tags object = {}

param applicationLandingZone object

@description('Container image tag or version')
param containerImageTag string = 'latest'

@description('Container registry server')
param containerRegistryServer string

var serviceName = toLower('${tags.Project}-${tags.Service}')
var defaultResourceName = toLower('${serviceName}-${tags.Environment}')
var resourceGroupName = '${defaultResourceName}-rg'

// Create Resource Group for ChatClient service
resource targetResourceGroup 'Microsoft.Resources/resourceGroups@2021-04-01' = {
  name: resourceGroupName
  location: location
  tags: tags
}

// Deploy ChatClient Container App
module chatClientApp 'resources.bicep' = {
  name: 'chatclient-app-deployment'
  scope: targetResourceGroup
  params: {
    serviceName: serviceName
    defaultResourceName: defaultResourceName
    location: location
    applicationLandingZone: applicationLandingZone
    containerImageTag: containerImageTag
    containerRegistryServer: containerRegistryServer
    tags: tags
  }
}

// Outputs
output resourceGroupName string = targetResourceGroup.name
output containerAppName string = chatClientApp.outputs.containerAppName
output containerAppUrl string = chatClientApp.outputs.containerAppUrl
output containerAppFqdn string = chatClientApp.outputs.containerAppFqdn
