param containerAppPrincipalId string
param systemName string

resource openAiUserRoleDefinition 'Microsoft.Authorization/roleDefinitions@2022-05-01-preview' existing = {
  name: '5e0bd9bd-7b93-4f28-af87-19fc36ad61bd'
}

module openAiUserRoleAssignment 'role-assignment.bicep' = {
  name: 'ra-${systemName}-${openAiUserRoleDefinition.name}'
  params: {
    principalId: containerAppPrincipalId
    roleDefinitionId: openAiUserRoleDefinition.id
  }
}
