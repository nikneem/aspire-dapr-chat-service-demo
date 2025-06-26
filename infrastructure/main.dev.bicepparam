using 'main.bicep'

// Development Environment Parameters
param location = 'West Europe'
param environment = 'dev'
param appName = 'hexchat'
param tags = {
  Environment: 'Development'
  Application: 'HexMaster Chat'
  CreatedBy: 'Bicep'
  Owner: 'Development Team'
  CostCenter: 'Engineering'
  Project: 'AspireChat'
}
