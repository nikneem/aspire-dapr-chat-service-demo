@description('The location for all resources')
param location string = resourceGroup().location

@description('Environment name (dev, staging, prod)')
param environment string

@description('Container App name')
param containerAppName string

param applicationLandingZone object

@description('Container image tag or version')
param containerImageTag string = 'latest'

@description('Container registry server')
param containerRegistryServer string

@description('Tags to apply to all resources')
param tags object = {}

resource containerAppsEnvironment 'Microsoft.App/managedEnvironments@2024-03-01' existing = {
  scope: resourceGroup(applicationLandingZone.resourceGroupName)
  name: applicationLandingZone.containerAppsEnvironmentName
}
resource azureAppConfiguration 'Microsoft.AppConfiguration/configurationStores@2024-06-15-preview' existing = {
  scope: resourceGroup(applicationLandingZone.resourceGroupName)
  name: applicationLandingZone.appConfigurationName
}
resource applicationInsights 'Microsoft.Insights/components@2020-02-02' existing = {
  scope: resourceGroup(applicationLandingZone.resourceGroupName)
  name: applicationLandingZone.applicationInsightsName
}

var containerImageName = '${containerRegistryServer}/cekeilholz/aspirichat-members-api:${containerImageTag}'
var storageAccountName = uniqueString(containerAppName)

var storageAccountConnectionString = 'DefaultEndpointsProtocol=https;AccountName=${storageAccountName};EndpointSuffix=core.windows.net'

resource storageAccount 'Microsoft.Storage/storageAccounts@2024-01-01' = {
  name: storageAccountName
  location: location
  tags: tags
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'StorageV2'
  properties: {
    supportsHttpsTrafficOnly: true
    isHnsEnabled: true
  }
  resource tableService 'tableServices' = {
    name: 'default'
    resource membersTable 'tables' = {
      name: 'members'
    }
  }
}

// Members API Container App
resource membersContainerApp 'Microsoft.App/containerApps@2024-03-01' = {
  name: containerAppName
  location: location
  tags: tags
  properties: {
    managedEnvironmentId: containerAppsEnvironment.id
    configuration: {
      activeRevisionsMode: 'Single'
      secrets: [
        {
          name: 'table-storage-connection-string'
          value: storageAccount.properties.primaryEndpoints.table
        }
        {
          name: 'appinsights-connection-string'
          value: applicationInsights.properties.ConnectionString
        }
      ]
      ingress: {
        external: true
        targetPort: 8080
        allowInsecure: false
        traffic: [
          {
            weight: 100
            latestRevision: true
          }
        ]
      }
      dapr: {
        enabled: true
        appId: 'members-api'
        appProtocol: 'http'
        appPort: 8080
        logLevel: 'info'
        enableApiLogging: true
      }
    }
    template: {
      containers: [
        {
          name: 'members-api'
          image: containerImageName
          env: [
            {
              name: 'ASPNETCORE_ENVIRONMENT'
              value: environment == 'prod' ? 'Production' : 'Development'
            }
            {
              name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
              secretRef: 'appinsights-connection-string'
            }
            {
              name: 'ConnectionStrings__AppConfig'
              value: azureAppConfiguration.properties.endpoint
            }
            {
              name: 'ConnectionStrings__tables'
              secretRef: 'table-storage-connection-string'
            }
            {
              name: 'HealthChecks__Enabled'
              value: 'true'
            }
          ]
          resources: {
            cpu: json('0.25')
            memory: '0.5Gi'
          }
          probes: [
            {
              type: 'Liveness'
              httpGet: {
                path: '/health'
                port: 8080
                scheme: 'HTTP'
              }
              initialDelaySeconds: 30
              periodSeconds: 30
              timeoutSeconds: 5
              failureThreshold: 3
            }
            {
              type: 'Readiness'
              httpGet: {
                path: '/health/ready'
                port: 8080
                scheme: 'HTTP'
              }
              initialDelaySeconds: 5
              periodSeconds: 10
              timeoutSeconds: 3
              failureThreshold: 3
            }
          ]
        }
      ]
      scale: {
        minReplicas: 1
        maxReplicas: 10
        rules: [
          {
            name: 'http-requests'
            http: {
              metadata: {
                concurrentRequests: '100'
              }
            }
          }
          {
            name: 'cpu-usage'
            custom: {
              type: 'cpu'
              metadata: {
                type: 'Utilization'
                value: '70'
              }
            }
          }
        ]
      }
    }
  }
}


resource tableDataContributorRoleDefinition 'Microsoft.Authorization/roleDefinitions@2022-04-01' existing = {
  name: '0a9a7e1f-b9d0-4cc4-a60d-0319b160aaa3'
}

resource tableDataContributorRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(resourcegroup().id, membersContainerApp.id, tableDataContributorRoleDefinition.id)
  properties: {
    roleDefinitionId: tableDataContributorRoleDefinition.id
    principalId: membersContainerApp.identity.principalId
    principalType: 'ServicePrincipal'
  }
}

// Outputs
output containerAppName string = membersContainerApp.name
output containerAppUrl string = 'https://${membersContainerApp.properties.configuration.ingress.fqdn}'
output containerAppFqdn string = membersContainerApp.properties.configuration.ingress.fqdn
output containerAppId string = membersContainerApp.id
