using 'main.bicep'

// Production Environment Parameters
param location = 'West Europe'
param environment = 'prod'
param appName = 'hexchat'
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
