param principalId string
param cosmosDbName string

resource cosmosDbService 'Microsoft.DocumentDB/databaseAccounts@2024-09-01-preview' existing = {
  name: cosmosDbName
}

resource sqlRoleAssignment 'Microsoft.DocumentDB/databaseAccounts/sqlRoleAssignments@2023-04-15' = {
  parent: cosmosDbService
  name: guid(principalId, cosmosDbService.id, 'reader')
  properties: {
    principalId: principalId
    roleDefinitionId: '/${subscription().id}/resourceGroups/${resourceGroup().name}/providers/Microsoft.DocumentDB/databaseAccounts/${cosmosDbService.name}/sqlRoleDefinitions/00000000-0000-0000-0000-000000000001'
    scope: cosmosDbService.id
  }
}
resource writerRole 'Microsoft.DocumentDB/databaseAccounts/sqlRoleAssignments@2024-09-01-preview' = {
  parent: cosmosDbService
  name: guid(principalId, cosmosDbService.id, 'writer')
  properties: {
    principalId: principalId
    roleDefinitionId: '/${subscription().id}/resourceGroups/${resourceGroup().name}/providers/Microsoft.DocumentDB/databaseAccounts/${cosmosDbService.name}/sqlRoleDefinitions/00000000-0000-0000-0000-000000000002'
    scope: cosmosDbService.id
  }
}
