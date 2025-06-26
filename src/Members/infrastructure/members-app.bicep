@description('The location for all resources')
param location string = resourceGroup().location

@description('Environment name (dev, staging, prod)')
param environment string

@description('Application name prefix')
param appName string

@description('Container App name')
param containerAppName string

@description('Container Apps Environment resource ID')
param containerAppsEnvironmentId string

@description('App Configuration endpoint')
param appConfigurationEndpoint string

@description('Application Insights connection string')
param applicationInsightsConnectionString string

@description('Container image tag or version')
param containerImageTag string = 'latest'

@description('Container registry server')
param containerRegistryServer string

@description('Container registry username')
@secure()
param containerRegistryUsername string

@description('Container registry password')
@secure()
param containerRegistryPassword string

@description('Azure Table Storage connection string')
@secure()
param tableStorageConnectionString string

@description('Dapr pub/sub component name')
param daprPubSubComponentName string

@description('Dapr state store component name')
param daprStateStoreComponentName string

@description('Tags to apply to all resources')
param tags object = {}

var containerImageName = '${containerRegistryServer}/hexmaster-chat-members-api:${containerImageTag}'

// Members API Container App
resource membersContainerApp 'Microsoft.App/containerApps@2024-03-01' = {
  name: containerAppName
  location: location
  tags: tags
  properties: {
    managedEnvironmentId: containerAppsEnvironmentId
    configuration: {
      activeRevisionsMode: 'Single'
      secrets: [
        {
          name: 'container-registry-password'
          value: containerRegistryPassword
        }
        {
          name: 'table-storage-connection-string'
          value: tableStorageConnectionString
        }
        {
          name: 'appinsights-connection-string'
          value: applicationInsightsConnectionString
        }
      ]
      registries: [
        {
          server: containerRegistryServer
          username: containerRegistryUsername
          passwordSecretRef: 'container-registry-password'
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
              name: 'ASPNETCORE_URLS'
              value: 'http://+:8080'
            }
            {
              name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
              secretRef: 'appinsights-connection-string'
            }
            {
              name: 'ConnectionStrings__AppConfig'
              value: appConfigurationEndpoint
            }
            {
              name: 'ConnectionStrings__TableStorage'
              secretRef: 'table-storage-connection-string'
            }
            {
              name: 'Dapr__PubSubComponentName'
              value: daprPubSubComponentName
            }
            {
              name: 'Dapr__StateStoreComponentName'
              value: daprStateStoreComponentName
            }
            {
              name: 'Dapr__AppId'
              value: 'members-api'
            }
            {
              name: 'Dapr__HttpEndpoint'
              value: 'http://localhost:3500'
            }
            {
              name: 'Dapr__GrpcEndpoint'
              value: 'http://localhost:50001'
            }
            {
              name: 'Logging__LogLevel__Default'
              value: environment == 'prod' ? 'Information' : 'Debug'
            }
            {
              name: 'Logging__LogLevel__Microsoft.AspNetCore'
              value: 'Warning'
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

// Outputs
output containerAppName string = membersContainerApp.name
output containerAppUrl string = 'https://${membersContainerApp.properties.configuration.ingress.fqdn}'
output containerAppFqdn string = membersContainerApp.properties.configuration.ingress.fqdn
output containerAppId string = membersContainerApp.id
