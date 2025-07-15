param containerAppPrincipalId string
param systemName string

resource openAiUserRoleDefinition 'Microsoft.Authorization/roleDefinitions@2022-05-01-preview' existing = {
  name: '5e0bd9bd-7b93-4f28-af87-19fc36ad61bd'
}

// Probably too much permissions
// resource openAiContributorRoleDefinition 'Microsoft.Authorization/roleDefinitions@2022-05-01-preview' existing = {
//   name: 'a001fd3d-188f-4b5d-821b-7da978bf7442'
// }

module openAiUserRoleAssignment 'role-assignment.bicep' = {
  name: 'ra-${systemName}-${openAiUserRoleDefinition.name}'
  params: {
    principalId: containerAppPrincipalId
    roleDefinitionId: openAiUserRoleDefinition.id
  }
}
