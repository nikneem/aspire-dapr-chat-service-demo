@description('Name of the service')
param serviceName string

@description('Default resource name prefix for all resources')
param defaultResourceName string

@description('The location for all resources')
param location string = resourceGroup().location

@description('Application landing zone configuration')
param applicationLandingZone object

@description('Container image tag or version')
param containerImageTag string

@description('Container registry server')
param containerRegistryServer string

@description('Tags to apply to all resources')
param tags object = {}

param containerPort int = 8080

var serviceBusTopics = [
  {
    name: 'message-sent'
  }
]

var containerImageName = '${containerRegistryServer}/cekeilholz/aspirichat-messages-api:${containerImageTag}'


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
module serviceBusTopicsModule '../../../infrastructure/shared/servicebus-topics.bicep' = {
  scope: resourceGroup(applicationLandingZone.resourceGroupName)
  name: '${serviceName}-sb-topics'
  params: {
    landingzoneEnvironment: applicationLandingZone
    topics: serviceBusTopics
  }
}

resource storageAccount 'Microsoft.Storage/storageAccounts@2024-01-01' = {
  name: uniqueString(defaultResourceName)
  location: location
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'StorageV2'
  tags: tags
  properties: {
    supportsHttpsTrafficOnly: true
    isHnsEnabled: true
    minimumTlsVersion: 'TLS1_2'
  }
  resource tableService 'tableServices' = {
    name: 'default'
    resource membersTable 'tables' = {
      name: 'messages'
    }
  }
}

// Members API Container App
resource apiContainerApp 'Microsoft.App/containerApps@2024-03-01' = {
  name: '${defaultResourceName}-app'
  location: location
  identity: {
    type: 'SystemAssigned'
  } 
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
        targetPort: containerPort
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
        appId: serviceName
        appProtocol: 'http'
        appPort: containerPort
        logLevel: 'info'
        enableApiLogging: true
      }
    }
    template: {
      containers: [
        {
          name: serviceName
          image: containerImageName
          env: [
            {
              name: 'ASPNETCORE_ENVIRONMENT'
              value: tags.Environment == 'Prod' ? 'Production' : 'Development'
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
              name: 'ConnectionStrings__messagestables'
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
                path: '/alive'
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

module appConfigRoleAssignment '../../../infrastructure/shared/role-assignment-app-configuration.bicep' = {
  scope: resourceGroup(applicationLandingZone.resourceGroupName)
  name: '${defaultResourceName}-appcfg-module'
  params: {
    containerAppPrincipalId: apiContainerApp.identity.principalId
    systemName: serviceName
  }
}
module tableDataRoleAssignment '../../../infrastructure/shared/role-assignment-table-data-contrib.bicep' = {
  name: '${defaultResourceName}-tablecontrib-module'
  params: {
    containerAppPrincipalId: apiContainerApp.identity.principalId
    systemName: serviceName
  }
}

output containerAppName string = apiContainerApp.name
output containerAppUrl string = 'https://${apiContainerApp.properties.configuration.ingress.fqdn}'
output containerAppFqdn string = apiContainerApp.properties.configuration.ingress.fqdn
output containerAppId string = apiContainerApp.id
