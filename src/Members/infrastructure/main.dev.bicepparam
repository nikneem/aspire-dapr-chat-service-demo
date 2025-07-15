using 'main.bicep'

// These values should be provided during deployment or from Azure DevOps/GitHub Actions variables
// Container Apps Environment ID from shared infrastructure deployment
param applicationLandingZone = {
  resourceGroupName: 'aspirichat-lanzingzn-dev-rg'
  containerAppsEnvironmentName: 'aspirichat-lanzingzn-cae-dev-o5774ipalwf62'
  appConfigurationName: 'aspirichat-lanzingzn-ac-dev-o5774ipalwf62'
  applicationInsightsName: 'aspirichat-lanzingzn-ai-dev-o5774ipalwf62'
  serviceBus: 'aspirichat-lanzingzn-sb-dev-o5774ipalwf62'
}


// Container configuration
param containerRegistryServer = 'docker.io'

param tags = {
  Environment: 'Dev'
  Application: 'HexMaster Chat'
  Service: 'Members'
  CreatedBy: 'Bicep'
  Owner: 'Development Team'
  CostCenter: 'Engineering'
  Project: 'AspiChat'
}
