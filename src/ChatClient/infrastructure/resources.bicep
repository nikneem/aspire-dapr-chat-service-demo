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

param containerPort int = 3000

var containerImageName = '${containerRegistryServer}/cekeilholz/aspirichat-client:${containerImageTag}'

resource containerAppsEnvironment 'Microsoft.App/managedEnvironments@2024-03-01' existing = {
  scope: resourceGroup(applicationLandingZone.resourceGroupName)
  name: applicationLandingZone.containerAppsEnvironmentName
}

// resource containerAppDomainCertificate'Microsoft.App/managedEnvironments/certificates@2024-03-01' = {
//   name: 'chat-hexmaster-nl'
//   location: location
//   properties:{password:}
// }

resource clientContainerApp 'Microsoft.App/containerApps@2025-02-02-preview' = {
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
        customDomains: [
          {
            name: 'chat.hexmaster.nl'
            bindingType: 'Auto'
          }
        ]
      }

    }
    template: {
      containers: [
        {
          name: serviceName
          image: containerImageName
          env: [
            {
              name: 'NODE_ENV'
              value: 'production'
            }
            {
              name: 'PORT'
              value: '3000'
            }
          ]
          resources: {
            cpu: json('0.25')
            memory: '0.5Gi'
          }
        }
      ]
      scale: {
        minReplicas: 1
        maxReplicas: 5
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

module managedCertificate '../../../infrastructure/shared/container-apps-managed-cert.bicep' = {
  scope : resourceGroup(applicationLandingZone.resourceGroupName)
  name: 'chatClientManagedCertificate'
  params: {
    certificateName: 'chat-hexmaster-nl'
    location: location
    containerAppManegedEnvironmentName: applicationLandingZone.containerAppsEnvironmentName
    domainName: 'chat.hexmaster.nl'
  }
}

output containerAppName string = clientContainerApp.name
output containerAppUrl string = 'https://${clientContainerApp.properties.configuration.ingress.fqdn}'
output containerAppFqdn string = clientContainerApp.properties.configuration.ingress.fqdn
output containerAppId string = clientContainerApp.id
