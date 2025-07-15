param containerAppPrincipalId string
param systemName string

resource tableDataContributorRoleDefinition 'Microsoft.Authorization/roleDefinitions@2022-05-01-preview' existing = {
  name: '0a9a7e1f-b9d0-4cc4-a60d-0319b160aaa3'
}

module tableDataContributorRoleAssignment 'role-assignment.bicep' = {
  name: 'ra-${systemName}-${tableDataContributorRoleDefinition.name}'
  params: {
    principalId: containerAppPrincipalId
    roleDefinitionId: tableDataContributorRoleDefinition.id
  }
}
