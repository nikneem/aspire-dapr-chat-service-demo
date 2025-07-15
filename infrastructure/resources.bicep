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
  tags: tags
  kind: 'web'
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
  tags: tags
  sku: {
    name: 'Standard'
    tier: 'Standard'
  }
  properties: {
    minimumTlsVersion: '1.2'
    publicNetworkAccess: 'Enabled'
    disableLocalAuth: false
    zoneRedundant: false
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
}

// Azure App Configuration (Free tier)
resource appConfiguration 'Microsoft.AppConfiguration/configurationStores@2023-03-01' = {
  name: appConfigName
  location: location
  tags: tags
  sku: {
    name: 'free'
  }
  properties: {
    encryption: {
      keyVaultProperties: null
    }
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
