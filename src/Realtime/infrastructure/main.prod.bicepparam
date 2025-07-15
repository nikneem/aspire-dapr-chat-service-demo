using 'main.bicep'

// Production Environment Parameters for Members Service
param environment = 'prod'
param appName = 'aspirichat'

param applicationLandingZone = {
  resourceGroupName: 'aspirichat-lanzingzn-dev-rg'
  containerAppsEnvironmentName: 'aspirichat-cae-dev-jl2mqo4oottpk'
  appConfigurationName: 'aspirichat-ac-dev-jl2mqo4oottpk'
  applicationInsightsName: 'aspirichat-ai-dev-jl2mqo4oottpk'
}

// Container configuration
param containerImageTag = 'stable'
param containerRegistryServer = 'docker.io'

param tags = {
  Environment: 'Production'
  Application: 'HexMaster Chat'
  Service: 'Realtime'
  CreatedBy: 'Bicep'
  Owner: 'Production Team'
  CostCenter: 'Engineering'
  Project: 'AspireChat'
}
