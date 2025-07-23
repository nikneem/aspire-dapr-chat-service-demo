@description('The location for all resources')
param location string = resourceGroup().location

@description('Environment name (dev, staging, prod)')
param environment string = 'dev'

@description('Application name prefix')
param appName string

@description('Tags to apply to all resources')
param tags object = {
  Environment: environment
  Application: 'HexMaster Chat'
  CreatedBy: 'Bicep'
}

// Generate unique names
var uniqueSuffix = uniqueString(resourceGroup().id)
var serviceBusNamespaceName = '${appName}-sb-${environment}-${uniqueSuffix}'
var containerAppsEnvironmentName = '${appName}-cae-${environment}-${uniqueSuffix}'
var logAnalyticsWorkspaceName = '${appName}-law-${environment}-${uniqueSuffix}'
var appInsightsName = '${appName}-ai-${environment}-${uniqueSuffix}'
var appConfigName = '${appName}-ac-${environment}-${uniqueSuffix}'
var redisCacheName = '${appName}-redis-${environment}-${uniqueSuffix}'

var daprStateStoreName = 'chatservice-statestore'
var daprPubSubName = 'chatservice-pubsub'

// Log Analytics Workspace
resource logAnalyticsWorkspace 'Microsoft.OperationalInsights/workspaces@2023-09-01' = {
  name: logAnalyticsWorkspaceName
  location: location
  tags: tags
  properties: {
    sku: {
      name: 'PerGB2018'
    }
    retentionInDays: 30
    features: {
      enableLogAccessUsingOnlyResourcePermissions: true
    }
  }
}

// Application Insights
resource applicationInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: appInsightsName
  location: location
  kind: 'web'
  tags: tags
  properties: {
    Application_Type: 'web'
    WorkspaceResourceId: logAnalyticsWorkspace.id
    IngestionMode: 'LogAnalytics'
    publicNetworkAccessForIngestion: 'Enabled'
    publicNetworkAccessForQuery: 'Enabled'
  }
}

// Azure Service Bus Namespace (Standard tier)
resource serviceBusNamespace 'Microsoft.ServiceBus/namespaces@2022-10-01-preview' = {
  name: serviceBusNamespaceName
  location: location
  sku: {
    name: 'Standard'
    tier: 'Standard'
  }
  tags: tags
  properties: {
    minimumTlsVersion: '1.2'
    publicNetworkAccess: 'Enabled'
    disableLocalAuth: false
    zoneRedundant: false
  }
    resource accessPolicies 'AuthorizationRules' = {
    name: 'DaprComponentPolicy'
    properties: {
      rights: [
        'Send'
        'Listen'
        'Manage'
      ]
    }
  }
}

// Container Apps Environment
resource containerAppsEnvironment 'Microsoft.App/managedEnvironments@2024-03-01' = {
  name: containerAppsEnvironmentName
  location: location
  tags: tags
  properties: {
    appLogsConfiguration: {
      destination: 'log-analytics'
      logAnalyticsConfiguration: {
        customerId: logAnalyticsWorkspace.properties.customerId
        sharedKey: logAnalyticsWorkspace.listKeys().primarySharedKey
      }
    }
    daprAIInstrumentationKey: applicationInsights.properties.InstrumentationKey
    daprAIConnectionString: applicationInsights.properties.ConnectionString
    workloadProfiles: [
      {
        name: 'Consumption'
        workloadProfileType: 'Consumption'
      }
    ]
    zoneRedundant: false
  }
    resource stateStoreComponent 'daprComponents' = {
    name: daprStateStoreName
    properties: {
      componentType: 'state.redis'
      version: 'v1'
      secrets: [
        {
          name: 'redispassword'
          value: redisCache.listKeys().primaryKey
        }
      ]
      metadata: [
        {
          name: 'redisHost'
          value: '${redisCache.properties.hostName}:${redisCache.properties.sslPort}'
        }
        {
          name: 'redisDB'
          value: '0'
        }
        {
          name: 'redisPassword'
          secretRef: 'redispassword'
        }
        {
          name: 'enableTLS'
          value: 'true'
        }
        {
          name: 'keyPrefix'
          value: 'none'
        }
      ]
    }
  }
  resource pubsubComponent 'daprComponents' = {
    name: daprPubSubName
    properties: {
      componentType: 'pubsub.azure.servicebus.topics'
      version: 'v1'
      secrets: [
        {
          name: 'servicebusnamespace'
          value: serviceBusNamespace::accessPolicies.listKeys().primaryConnectionString
        }
      ]
      metadata: [
        {
          name: 'connectionString'
          secretRef: 'servicebusnamespace'
        }
        {
          name: 'maxConcurrentHandlers'
          value: '3'
        }
      ]
    }
  }
}

// Azure App Configuration (Free tier)
resource appConfiguration 'Microsoft.AppConfiguration/configurationStores@2023-03-01' = {
  name: appConfigName
  location: location
  sku: {
    name: 'free'
  }
  tags: tags
  properties: {
    disableLocalAuth: false
    enablePurgeProtection: false
    publicNetworkAccess: 'Enabled'
  }
}

// Azure Cache for Redis (Basic C0 - cheapest tier)
resource redisCache 'Microsoft.Cache/redis@2024-03-01' = {
  name: redisCacheName
  location: location
  tags: tags
  properties: {
    sku: {
      name: 'Basic'
      family: 'C'
      capacity: 0
    }
    enableNonSslPort: false
    minimumTlsVersion: '1.2'
    publicNetworkAccess: 'Enabled'
    redisConfiguration: {
      'maxmemory-policy': 'volatile-lru'
    }
  }
}

// Outputs
output serviceBusNamespaceName string = serviceBusNamespace.name
output serviceBusNamespaceEndpoint string = serviceBusNamespace.properties.serviceBusEndpoint

output containerAppsEnvironmentId string = containerAppsEnvironment.id
output containerAppsEnvironmentName string = containerAppsEnvironment.name

output logAnalyticsWorkspaceId string = logAnalyticsWorkspace.id
output logAnalyticsWorkspaceName string = logAnalyticsWorkspace.name

output applicationInsightsConnectionString string = applicationInsights.properties.ConnectionString
output applicationInsightsInstrumentationKey string = applicationInsights.properties.InstrumentationKey

output appConfigurationName string = appConfiguration.name
output appConfigurationEndpoint string = appConfiguration.properties.endpoint

output redisCacheName string = redisCache.name
output redisCacheHostName string = redisCache.properties.hostName
output redisCachePort string = string(redisCache.properties.port)
output redisCacheSslPort string = string(redisCache.properties.sslPort)
