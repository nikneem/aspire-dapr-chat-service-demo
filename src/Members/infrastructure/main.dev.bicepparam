using 'main.bicep'

// Development Environment Parameters for Members Service
param location = 'North Europe'
param environment = 'dev'
param appName = 'aspirichat'

// These values should be provided during deployment or from Azure DevOps/GitHub Actions variables
// Container Apps Environment ID from shared infrastructure deployment
param applicationLandingZone = {
  resourceGroupName: 'aspirichat-dev-rg'
  containerAppsEnvironmentName: 'aspirichat-cae-dev-jl2mqo4oottpk'
  appConfigurationName: 'aspirichat-ac-dev-jl2mqo4oottpk'
  applicationInsightsName: 'aspirichat-ai-dev-jl2mqo4oottpk'
}


// Container configuration
param containerImageTag = 'latest'
param containerRegistryServer = 'docker.io'

param tags = {
  Environment: 'Development'
  Application: 'HexMaster Chat'
  Service: 'Members'
  CreatedBy: 'Bicep'
  Owner: 'Development Team'
  CostCenter: 'Engineering'
  Project: 'AspireChat'
}
