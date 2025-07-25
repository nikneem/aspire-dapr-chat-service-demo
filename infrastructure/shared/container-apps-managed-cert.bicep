param certificateName string
param location string = resourceGroup().location
param containerAppManegedEnvironmentName string
param domainName string 
resource managedEnvironment 'Microsoft.App/managedEnvironments@2025-02-02-preview' existing = {
  name: containerAppManegedEnvironmentName
}

resource managedCertificate 'Microsoft.App/managedEnvironments/managedCertificates@2025-02-02-preview' = {
  parent: managedEnvironment
  name: certificateName
  location: location
  properties: {
    domainControlValidation: 'CNAME'
    subjectName: domainName
  }
}
