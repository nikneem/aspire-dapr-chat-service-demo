param containerAppsEnvironmentName string
param daprComponentName string
param daprComponentType string
param daprComponentVersion string = 'v1'

param secrets array = []
param componentMetadata array = []
param scopes array = []

resource containerAppsEnvironment 'Microsoft.App/managedEnvironments@2024-08-02-preview' existing = {
  name: containerAppsEnvironmentName
}

resource daprComponent 'Microsoft.App/managedEnvironments/daprComponents@2024-03-01' = {
  name: daprComponentName
  parent: containerAppsEnvironment
  properties: {
    componentType: daprComponentType
    version: daprComponentVersion
    secrets: secrets
    metadata: componentMetadata
    scopes: scopes
  }
}
