param containerAppPrincipalId string
param systemName string

resource tableBlobContributorRoleDefinition 'Microsoft.Authorization/roleDefinitions@2022-05-01-preview' existing = {
  name: 'ba92f5b4-2d11-453d-a403-e96b0029c9fe'
}

module tableBlobContributorRoleAssignment 'role-assignment.bicep' = {
  name: 'ra-${systemName}-${tableBlobContributorRoleDefinition.name}'
  params: {
    principalId: containerAppPrincipalId
    roleDefinitionId: tableBlobContributorRoleDefinition.id
  }
}
