param appConfigurationName string
param settingName string
param settingValue string

resource appConfiguration 'Microsoft.AppConfiguration/configurationStores@2023-03-01' existing = {
  name: appConfigurationName
}

resource configurationValue 'Microsoft.AppConfiguration/configurationStores/keyValues@2023-03-01' = {
  name: settingName
  parent: appConfiguration
  properties: {
    contentType: 'text/plain'
    value: settingValue
  }
}
