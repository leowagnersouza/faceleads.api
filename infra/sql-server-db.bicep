targetScope = 'resourceGroup'

@description('Name of the SQL Server resource (must be globally unique)')
param sqlServerName string = 'faceleads-sql-dev'

@description('Location for resources (defaults to resource group location)')
param location string = resourceGroup().location

@description('Administrator login name for SQL Server')
param administratorLogin string = 'sqladmin'

@description('Administrator password for SQL Server (secure)')
@secure()
param administratorLoginPassword string

@description('Name of the database to create')
param databaseName string = 'Faceleads'

@description('SKU for the database (Basic, S0, GP_Gen5_2, etc). Use Basic for low cost dev.')
param databaseSku string = 'Basic'

@description('If set, create a firewall rule to allow this client IP (start). Empty disables')
param firewallStartIp string = ''

@description('If set, end IP for firewall rule. If empty uses start IP')
param firewallEndIp string = ''

@description('Create server-level firewall rule to allow Azure services (0.0.0.0)')
param allowAzureServices bool = true

// SQL Server
resource sqlServer 'Microsoft.Sql/servers@2021-08-01-preview' = {
  name: sqlServerName
  location: location
  properties: {
    administratorLogin: administratorLogin
    administratorLoginPassword: administratorLoginPassword
    version: '12.0'
  }
}

// Database
resource sqlDb 'Microsoft.Sql/servers/databases@2021-08-01-preview' = {
  name: '${sqlServer.name}/${databaseName}'
  properties: {
    collation: 'SQL_Latin1_General_CP1_CI_AS'
  }
  sku: {
    name: databaseSku
  }
  dependsOn: [
    sqlServer
  ]
}

// Optional firewall rule for client IP
resource clientFw 'Microsoft.Sql/servers/firewallRules@2021-08-01-preview' = if (firewallStartIp != '') {
  name: '${sqlServer.name}/AllowClientIP'
  properties: {
    startIpAddress: firewallStartIp
    endIpAddress: empty(firewallEndIp) ? firewallStartIp : firewallEndIp
  }
  dependsOn: [ sqlServer ]
}

// Optional allow Azure services (0.0.0.0)
resource allowAzure 'Microsoft.Sql/servers/firewallRules@2021-08-01-preview' = if (allowAzureServices) {
  name: '${sqlServer.name}/AllowAzureServices'
  properties: {
    startIpAddress: '0.0.0.0'
    endIpAddress: '0.0.0.0'
  }
  dependsOn: [ sqlServer ]
}

output sqlServerFqdn string = '${sqlServer.name}.database.windows.net'
output databaseNameOut string = sqlDb.name
output connectionString string = 'Server=tcp:${sqlServer.name}.database.windows.net,1433;Initial Catalog=${databaseName};Persist Security Info=False;User ID=${administratorLogin};Password=${administratorLoginPassword};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;'
