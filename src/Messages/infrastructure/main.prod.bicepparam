using 'main.bicep'

param applicationLandingZone = {
  resourceGroupName: 'aspirichat-lanzingzn-dev-rg'
  containerAppsEnvironmentName: 'aspirichat-cae-dev-jl2mqo4oottpk'
  appConfigurationName: 'aspirichat-ac-dev-jl2mqo4oottpk'
  applicationInsightsName: 'aspirichat-ai-dev-jl2mqo4oottpk'
}

// Container configuration
param containerRegistryServer = 'docker.io'

param tags = {
  Environment: 'Prod'
  Application: 'HexMaster Chat'
  Service: 'Messages'
  CreatedBy: 'Bicep'
  Owner: 'Production Team'
  CostCenter: 'Engineering'
  Project: 'AspiChat'
}
