using 'main.bicep'

// Production Environment Parameters
param environment = 'prod'
param appName = 'aspirichat-lanzingzn'
param tags = {
  Environment: 'Production'
  Application: 'HexMaster Chat'
  CreatedBy: 'Bicep'
  Owner: 'Operations Team'
  CostCenter: 'Production'
  Project: 'AspireChat'
  Criticality: 'High'
  BackupRequired: 'Yes'
}
