param containerAppPrincipalId string
param systemName string

resource configurationDataReaderRole 'Microsoft.Authorization/roleDefinitions@2018-01-01-preview' existing = {
  name: '516239f1-63e1-4d78-a4de-a74fb236a071'
}
resource keyVaultSecretsUserRoleDefinition 'Microsoft.Authorization/roleDefinitions@2018-01-01-preview' existing = {
  name: '4633458b-17de-408a-b874-0445c86b69e6'
}

module configurationReaderRoleAssignment 'role-assignment.bicep' = {
  name: 'ra-${systemName}-${configurationDataReaderRole.name}'
  params: {
    principalId: containerAppPrincipalId
    roleDefinitionId: configurationDataReaderRole.id
  }
}

module keyVaultSecretsUserRoleAssignment 'role-assignment.bicep' = {
  name: 'ra-${systemName}-${keyVaultSecretsUserRoleDefinition.name}'
  params: {
    principalId: containerAppPrincipalId
    roleDefinitionId: keyVaultSecretsUserRoleDefinition.id
  }
}
