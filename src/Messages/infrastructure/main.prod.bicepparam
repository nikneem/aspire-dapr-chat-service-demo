using 'main.bicep'

param applicationLandingZone = {
  resourceGroupName: 'aspirichat-lanzingzn-dev-rg'
  containerAppsEnvironmentName: 'aspirichat-cae-dev-jl2mqo4oottpk'
  appConfigurationName: 'aspirichat-ac-dev-jl2mqo4oottpk'
  applicationInsightsName: 'aspirichat-ai-dev-jl2mqo4oottpk'
  serviceBus: 'aspirichat-lanzingzn-sb-dev-o5774ipalwf62'
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
